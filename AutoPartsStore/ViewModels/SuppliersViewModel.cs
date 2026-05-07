using System.Collections.ObjectModel;
using System.Linq;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.ViewModels
{
    public class SuppliersViewModel : BaseViewModel
    {
        public ObservableCollection<Suppliers> Suppliers { get; set; }

        private Suppliers _selectedSupplier;
        public Suppliers SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (_selectedSupplier != value)
                {
                    _selectedSupplier = value;
                    OnPropertyChanged();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    DeleteCommand?.RaiseCanExecuteChanged();
                    SaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public SuppliersViewModel(Users user)
        {
            Suppliers = new ObservableCollection<Suppliers>();
            AddCommand = new RelayCommand(_ => AddSupplier());
            DeleteCommand = new RelayCommand(_ => DeleteSupplier(), _ => SelectedSupplier != null);
            SaveCommand = new RelayCommand(_ => SaveChanges(), _ => SelectedSupplier != null);
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var list = db.Suppliers.ToList();
                Suppliers.Clear();
                foreach (var s in list) Suppliers.Add(s);
            }
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private void AddSupplier()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var newSup = new Suppliers { Name = "Новый поставщик" };
                db.Suppliers.Add(newSup);
                db.SaveChanges();
                Suppliers.Add(newSup);
                SnackbarService.Show("Поставщик добавлен");
            }
        }

        private void DeleteSupplier()
        {
            if (SelectedSupplier == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                bool hasReceivings = db.StockReceivings.Any(r => r.SupplierId == SelectedSupplier.SupplierId);
                if (hasReceivings)
                {
                    SnackbarService.Show("Невозможно удалить поставщика, так как существуют связанные поступления.");
                    return;
                }

                var supplier = db.Suppliers.Find(SelectedSupplier.SupplierId);
                if (supplier != null)
                {
                    db.Suppliers.Remove(supplier);
                    db.SaveChanges();
                    Suppliers.Remove(SelectedSupplier);
                    SnackbarService.Show("Поставщик удалён");
                }
            }
        }

        private void SaveChanges()
        {
            if (SelectedSupplier == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var supplier = db.Suppliers.Find(SelectedSupplier.SupplierId);
                if (supplier != null)
                {
                    supplier.Name = SelectedSupplier.Name;
                    supplier.ContactPerson = SelectedSupplier.ContactPerson;
                    supplier.Phone = SelectedSupplier.Phone;
                    supplier.Email = SelectedSupplier.Email;
                    db.SaveChanges();
                    SnackbarService.Show("Изменения сохранены");
                    LoadSuppliers();
                }
            }
        }
    }
}