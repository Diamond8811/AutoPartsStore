using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.ViewModels
{
    public class ReturnsViewModel : BaseViewModel
    {
        private readonly Users _currentUser;
        public ObservableCollection<Returns> Returns { get; set; }
        public Returns SelectedReturn { get; set; }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateReturnCommand { get; }

        public ReturnsViewModel(Users user)
        {
            _currentUser = user;
            Returns = new ObservableCollection<Returns>();
            RefreshCommand = new RelayCommand(_ => LoadReturnsAsync());
            CreateReturnCommand = new RelayCommand(_ => OpenCreateReturnWindow());
            LoadReturnsAsync();
        }

        private async void LoadReturnsAsync()
        {
            await ExecuteAsync("загрузки возвратов", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var list = db.Returns.Include("ReturnReasons").ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Returns.Clear();
                        foreach (var r in list) Returns.Add(r);
                    });
                }
            });
        }

        private void OpenCreateReturnWindow()
        {
            var window = new Views.CreateReturnWindow(_currentUser);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            LoadReturnsAsync();
        }
    }
}