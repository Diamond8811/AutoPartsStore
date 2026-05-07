using AutoPartsStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoPartsStore.Models;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoPartsStore.Views
{
    /// <summary>
    /// Логика взаимодействия для CreateReturnWindow.xaml
    /// </summary>
    public partial class CreateReturnWindow : Window
    {
        public CreateReturnWindow(Users currentUser)
        {
            InitializeComponent();
            DataContext = new CreateReturnViewModel(currentUser);
        }

        private void Close(object sender, RoutedEventArgs e) => this.Close();
    }
}
