using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using System.Windows;

namespace Group6WPF
{
    public partial class ManageCategory : Window
    {
        private readonly CategoryService categoryService;

        public ManageCategory(CategoryService categoryService)
        {
            InitializeComponent();
            this.categoryService = categoryService;

            LoadData();
        }

        private void LoadData()
        {
            dgCategories.ItemsSource = categoryService.GetAllCategories();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var keyword = txtSearch.Text.Trim();
            dgCategories.ItemsSource = categoryService.SearchCategories(keyword);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new CategoryForm();
            form.Owner = this;

            if (form.ShowDialog() == true)
            {
                categoryService.AddCategory(form.Category);
                LoadData();
            }
        }


        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCategory)
            {
                var form = new CategoryForm(selectedCategory);
                form.Owner = this;

                if (form.ShowDialog() == true)
                {
                    categoryService.UpdateCategory(form.Category);
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Please select a category to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCategory)
            {
                if (MessageBox.Show($"Are you sure you want to delete category '{selectedCategory.Name}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    categoryService.DeleteCategory(selectedCategory.CategoryID);
                    LoadData();
                }
            }
        }

    }
}
