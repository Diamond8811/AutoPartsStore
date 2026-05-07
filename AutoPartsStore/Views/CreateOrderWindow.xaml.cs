using AutoPartsStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoPartsStore.Models;
using System.Windows.Shapes;

namespace AutoPartsStore.Views
{
    /// <summary>
    /// Логика взаимодействия для CreateOrderWindow.xaml
    /// </summary>
    public partial class CreateOrderWindow : Window
    {
        public CreateOrderWindow(Users currentUser)
        {
            InitializeComponent();
            DataContext = new CreateOrderViewModel(currentUser);
        }

        private void Close(object sender, RoutedEventArgs e) => this.Close();
    }
}
