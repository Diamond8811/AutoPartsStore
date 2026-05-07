using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.ViewModels
{
    public class StockReceivingsViewModel : BaseViewModel
    {
        private readonly Users _currentUser;
        public ObservableCollection<StockReceivings> StockReceivings { get; set; }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateReceivingCommand { get; }

        public StockReceivingsViewModel(Users user)
        {
            _currentUser = user;
            StockReceivings = new ObservableCollection<StockReceivings>();
            RefreshCommand = new RelayCommand(_ => LoadReceivings());
            CreateReceivingCommand = new RelayCommand(_ => OpenCreateWindow());
            LoadReceivings();
        }

        private void LoadReceivings()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var list = db.StockReceivings.Include("Suppliers").Include("Users").ToList();
                StockReceivings.Clear();
                foreach (var r in list) StockReceivings.Add(r);
            }
        }

        private void OpenCreateWindow()
        {
            var win = new Views.CreateStockReceivingWindow();
            win.DataContext = new CreateStockReceivingViewModel(_currentUser);
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true)
                LoadReceivings();
        }
    }
}