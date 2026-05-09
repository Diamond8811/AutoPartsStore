using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoPartsStore.ViewModels
{
    public class EditProductViewModel : BaseViewModel
    {
        public Products Product { get; set; }
        public ObservableCollection<Brands> Brands { get; set; }
        public ObservableCollection<Categories> Categories { get; set; }

        public Brands SelectedBrand { get; set; }
        public Categories SelectedCategory { get; set; }

        public RelayCommand SaveCommand { get; }

        public EditProductViewModel(Products product)
        {
            Product = product;

            Brands = new ObservableCollection<Brands>();
            Categories = new ObservableCollection<Categories>();

            using (var db = new AutoPartsStoreDBEntities())
            {
                foreach (var b in db.Brands.ToList())
                    Brands.Add(b);

                foreach (var c in db.Categories.ToList())
                    Categories.Add(c);
            }

            SelectedBrand = Brands.FirstOrDefault(b => b.BrandId == Product.BrandId);
            SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == Product.CategoryId);

            SaveCommand = new RelayCommand(_ => Save());
        }

        private void Save()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var product = db.Products.Find(Product.ProductId);

                if (product != null)
                {
                    product.Article = Product.Article;
                    product.Name = Product.Name;
                    product.Price = Product.Price;
                    product.StockQuantity = Product.StockQuantity;

                    if (SelectedBrand != null)
                        product.BrandId = SelectedBrand.BrandId;

                    if (SelectedCategory != null)
                        product.CategoryId = SelectedCategory.CategoryId;

                    db.SaveChanges();
                }
            }

            SnackbarService.Show("Товар обновлён");
            System.Windows.Application.Current.Windows
                .OfType<Views.EditProductWindow>()
                .FirstOrDefault()?.Close();
        }
    }
}