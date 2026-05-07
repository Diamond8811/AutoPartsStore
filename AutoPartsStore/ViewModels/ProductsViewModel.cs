using System;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using AutoPartsStore.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AutoPartsStore.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly Users _currentUser;
        private string _filterArticle;
        private string _filterName;
        private Brands _selectedBrand;
        private Categories _selectedCategory;
        private decimal? _priceFrom;
        private decimal? _priceTo;
        private bool _onlyInStock;

        public ObservableCollection<Products> Products { get; set; }
        public ObservableCollection<Brands> Brands { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }
        public Products SelectedProduct { get; set; }

        public string FilterArticle
        {
            get => _filterArticle;
            set { _filterArticle = value; OnPropertyChanged(); }
        }
        public string FilterName
        {
            get => _filterName;
            set { _filterName = value; OnPropertyChanged(); }
        }
        public Brands SelectedBrand
        {
            get => _selectedBrand;
            set { _selectedBrand = value; OnPropertyChanged(); }
        }
        public Categories SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }
        public decimal? PriceFrom
        {
            get => _priceFrom;
            set { _priceFrom = value; OnPropertyChanged(); }
        }
        public decimal? PriceTo
        {
            get => _priceTo;
            set { _priceTo = value; OnPropertyChanged(); }
        }
        public bool OnlyInStock
        {
            get => _onlyInStock;
            set { _onlyInStock = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand FilterCommand { get; }
        public RelayCommand ResetFilterCommand { get; }
        public RelayCommand ImportExcelCommand { get; }
        public RelayCommand ExportExcelCommand { get; }

        public ProductsViewModel(Users user)
        {
            _currentUser = user;
            Products = new ObservableCollection<Products>();
            Brands = new ObservableCollection<Brands>();
            Categories = new ObservableCollection<Categories>();

            AddCommand = new RelayCommand(_ => AddProduct());
            DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            SaveCommand = new RelayCommand(_ => SaveChangesAsync());
            FilterCommand = new RelayCommand(_ => LoadFilteredProductsAsync());
            ResetFilterCommand = new RelayCommand(_ => ResetFilters());
            ImportExcelCommand = new RelayCommand(_ => ImportExcel());
            ExportExcelCommand = new RelayCommand(_ => ExportExcel());

            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            await ExecuteAsync("загрузки справочников", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var brandsList = db.Brands.ToList();
                    var categoriesList = db.Categories.ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Brands.Clear();
                        foreach (var b in brandsList) Brands.Add(b);
                        Categories.Clear();
                        foreach (var c in categoriesList) Categories.Add(c);
                    });
                }
            });
            await LoadFilteredProductsAsync();
        }

        private async Task LoadFilteredProductsAsync()
        {
            await ExecuteAsync("фильтрации товаров", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    var query = db.Products.Include("Brands").Include("Categories").AsQueryable();
                    if (!string.IsNullOrWhiteSpace(FilterArticle))
                        query = query.Where(p => p.Article.Contains(FilterArticle));
                    if (!string.IsNullOrWhiteSpace(FilterName))
                        query = query.Where(p => p.Name.Contains(FilterName));
                    if (SelectedBrand != null)
                        query = query.Where(p => p.BrandId == SelectedBrand.BrandId);
                    if (SelectedCategory != null)
                        query = query.Where(p => p.CategoryId == SelectedCategory.CategoryId);
                    if (PriceFrom.HasValue)
                        query = query.Where(p => p.Price >= PriceFrom.Value);
                    if (PriceTo.HasValue)
                        query = query.Where(p => p.Price <= PriceTo.Value);
                    if (OnlyInStock)
                        query = query.Where(p => p.StockQuantity > 0);

                    var result = query.ToList();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Products.Clear();
                        foreach (var p in result) Products.Add(p);
                    });
                }
            });
        }

        private void ResetFilters()
        {
            FilterArticle = null;
            FilterName = null;
            SelectedBrand = null;
            SelectedCategory = null;
            PriceFrom = null;
            PriceTo = null;
            OnlyInStock = false;
            LoadFilteredProductsAsync();
        }

        private void AddProduct()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var newProduct = new Products
                {
                    Article = "NEW_" + DateTime.Now.Ticks.ToString(), // уникальный артикул
                    Name = "Новый товар",
                    Price = 0,
                    StockQuantity = 0,
                    BrandId = db.Brands.First().BrandId,
                    CategoryId = db.Categories.First().CategoryId
                };
                db.Products.Add(newProduct);
                db.SaveChanges();
                Products.Add(newProduct);
                SnackbarService.Show("Товар добавлен");
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;
            using (var db = new AutoPartsStoreDBEntities())
            {
                var product = db.Products.Find(SelectedProduct.ProductId);
                if (product != null)
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                    Products.Remove(SelectedProduct);
                    SnackbarService.Show("Товар удалён");
                }
            }
        }

        private async void SaveChangesAsync()
        {
            await ExecuteAsync("сохранения товаров", () =>
            {
                using (var db = new AutoPartsStoreDBEntities())
                {
                    foreach (var product in Products)
                        db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                }
                SnackbarService.Show("Изменения сохранены");
            });
        }

        private async void ImportExcel()
        {
            var dialog = new OpenFileDialog { Filter = "Excel files (*.xlsx)|*.xlsx" };
            if (dialog.ShowDialog() == true)
            {
                await ExecuteAsync("импорта товаров", () =>
                {
                    var service = new ExcelImportService();
                    var (added, updated) = service.ImportProducts(dialog.FileName, _currentUser.UserId);
                    SnackbarService.Show($"Импорт завершён: добавлено {added}, обновлено {updated}");
                    LoadFilteredProductsAsync().Wait();
                });
            }
        }

        private async void ExportExcel()
        {
            await ExecuteAsync("экспорта товаров", () =>
            {
                var service = new ExcelService();
                service.ExportProducts(Products.ToList());
                SnackbarService.Show($"Экспортировано {Products.Count} товаров");
            });
        }
    }
}