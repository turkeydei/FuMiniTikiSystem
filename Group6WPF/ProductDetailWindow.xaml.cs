using Firebase.Storage;
using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Group6WPF
{
    /// <summary>
    /// Interaction logic for ProductDetailWindow.xaml
    /// </summary>
    public partial class ProductDetailWindow : Window
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        public Product _product { get; set; }

        public ProductDetailWindow(ProductService productService, CategoryService categoryService)
        {
            InitializeComponent();
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService;

            btnBackToAdmin.Click += (s, e) => this.Close();
            btnSave.Click += BtnSave_Click;
        }

        private async void BtnBrowsePicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Product Image";
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filePath);

                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open))
                    {
                        var task = new FirebaseStorage(
                                "court-callers.appspot.com", // your Firebase Storage bucket
                                new FirebaseStorageOptions
                                {
                                    ThrowOnCancel = true
                                })
                            .Child("ProductImages")
                            .Child(fileName)
                            .PutAsync(stream);

                        string downloadUrl = await task;

                        // Update UI with download URL
                        txtPicture.Text = downloadUrl;

                        // Show preview in Image control
                        imgPreview.Source = new BitmapImage(new Uri(downloadUrl, UriKind.Absolute));

                        MessageBox.Show("Image uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Image upload failed:\n" + ex.Message, "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                cbCategory.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtPicture.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Giá sản phẩm không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int categoryId = (int)cbCategory.SelectedValue;
            try
            {
                if (_product == null)
                {

                    var newProduct = new Product
                    {
                        Name = txtProductName.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Price = price,
                        CategoryID = categoryId,
                        Picture = txtPicture.Text.Trim()
                    };
                    _productService.AddProduct(newProduct);
                    MessageBox.Show("Thêm sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {

                    _product.Name = txtProductName.Text.Trim();
                    _product.Description = txtDescription.Text.Trim();
                    _product.Price = price;
                    _product.CategoryID = categoryId;
                    _product.Picture = txtPicture.Text.Trim();
                    _productService.UpdateProduct(_product);
                    MessageBox.Show("Cập nhật sản phẩm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_product != null)
            {
                txtProductName.Text = _product.Name;
                txtDescription.Text = _product.Description;
                txtPrice.Text = _product.Price.ToString();
                txtPicture.Text = _product.Picture;
                cbCategory.SelectedValue = _product.CategoryID;

                if (!string.IsNullOrWhiteSpace(_product.Picture))
                {
                    try
                    {
                        imgPreview.Source = new BitmapImage(new Uri(_product.Picture, UriKind.Absolute));
                    }
                    catch
                    {
                        imgPreview.Source = null;
                    }
                }
            }
            else
            {
                cbCategory.SelectedIndex = 1;
            }

            cbCategory.ItemsSource = _categoryService.GetAllCategories();
        }
    }
}
