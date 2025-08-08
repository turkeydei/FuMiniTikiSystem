using Firebase.Storage;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Group6WPF
{
    public partial class CategoryForm : Window
    {
        public Category Category { get; private set; }

        public CategoryForm(Category category = null)
        {
            InitializeComponent();

            if (category != null)
            {
                Category = new Category
                {
                    CategoryID = category.CategoryID,
                    Name = category.Name,
                    Description = category.Description,
                    Picture = category.Picture,
                    IsActive = category.IsActive
                };

                txtName.Text = Category.Name;
                txtDescription.Text = Category.Description;
                txtPicture.Text = Category.Picture;
                chkIsActive.IsChecked = Category.IsActive ?? true;

                if (!string.IsNullOrWhiteSpace(Category.Picture))
                {
                    try
                    {
                        imgPreview.Source = new BitmapImage(new Uri(Category.Picture, UriKind.Absolute));
                    }
                    catch
                    {
                        imgPreview.Source = null;
                    }
                }
            }
            else
            {
                Category = new Category();
                chkIsActive.IsChecked = true;
            }
        }
        private async void BtnBrowsePicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Category Image",
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filePath);

                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open))
                    {
                        var task = new FirebaseStorage(
                                "court-callers.appspot.com", // your Firebase bucket
                                new FirebaseStorageOptions
                                {
                                    ThrowOnCancel = true
                                })
                            .Child("CategoryImages")
                            .Child(fileName)
                            .PutAsync(stream);

                        string downloadUrl = await task;

                        txtPicture.Text = downloadUrl;
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
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Category.Name = txtName.Text.Trim();
            Category.Description = txtDescription.Text.Trim();
            Category.Picture = txtPicture.Text.Trim();
            Category.IsActive = chkIsActive.IsChecked ?? true;

            //var context = new ValidationContext(Category, serviceProvider: null, items: null);
            //var results = new List<ValidationResult>();

            //if (!Validator.TryValidateObject(Category, context, results, true))
            //{
            //    string errors = string.Join("\n", results.Select(r => r.ErrorMessage));
            //    MessageBox.Show(errors, "Validation Errors", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
