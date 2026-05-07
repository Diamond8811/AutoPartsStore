using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.ViewModels
{
    public class CustomersViewModel : BaseViewModel
    {
        public ObservableCollection<Customers> Customers { get; set; }

        private Customers _selectedCustomer;
        public Customers SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                    DeleteCommand?.RaiseCanExecuteChanged();
                    SaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SearchCommand { get; }

        public CustomersViewModel(Users user)
        {
            Customers = new ObservableCollection<Customers>();
            AddCommand = new RelayCommand(_ => AddCustomer());
            DeleteCommand = new RelayCommand(_ => DeleteCustomer(), _ => SelectedCustomer != null);
            SaveCommand = new RelayCommand(_ => SaveChanges(), _ => SelectedCustomer != null);
            SearchCommand = new RelayCommand(_ => LoadCustomersAsync());

            LoadCustomersAsync();
        }

        private async void LoadCustomersAsync()
        {
            await ExecuteAsync("загрузки клиентов", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var query = db.Customers.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(SearchText))
                        query = query.Where(c => c.FullName.Contains(SearchText) ||
                                                 c.Phone.Contains(SearchText) ||
                                                 (c.Email != null && c.Email.Contains(SearchText)));
                    var list = query.ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Customers.Clear();
                        foreach (var c in list) Customers.Add(c);
                        CommandManager.InvalidateRequerySuggested();
                        DeleteCommand?.RaiseCanExecuteChanged();
                        SaveCommand?.RaiseCanExecuteChanged();
                    });
                }
            });
        }

        private void AddCustomer()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var newCustomer = new Customers { FullName = "Новый клиент", Phone = "" };
                db.Customers.Add(newCustomer);
                db.SaveChanges();
                Customers.Add(newCustomer);
                SnackbarService.Show("Клиент добавлен");
            }
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var customer = db.Customers.Find(SelectedCustomer.CustomerId);
                if (customer != null)
                {
                    db.Customers.Remove(customer);
                    db.SaveChanges();
                    Customers.Remove(SelectedCustomer);
                    SnackbarService.Show("Клиент удалён");
                }
                else
                {
                    SnackbarService.Show("Клиент не найден в базе");
                }
            }
        }

        private void SaveChanges()
        {
            if (SelectedCustomer == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var customer = db.Customers.Find(SelectedCustomer.CustomerId);
                if (customer != null)
                {
                    customer.FullName = SelectedCustomer.FullName;
                    customer.Phone = SelectedCustomer.Phone;
                    customer.Email = SelectedCustomer.Email;
                    customer.Address = SelectedCustomer.Address;
                    db.SaveChanges();
                    SnackbarService.Show("Изменения сохранены");
                    LoadCustomersAsync();
                }
            }
        }
    }
}