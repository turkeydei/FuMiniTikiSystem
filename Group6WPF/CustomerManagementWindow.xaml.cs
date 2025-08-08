using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Group6WPF
{
    public partial class CustomerManagementWindow : Window
    {
        private readonly CustomerService customerService;
        private ObservableCollection<Customer> customers;
        public Customer Account { get; set; }

        public string searchPlaceholder = "Search customers...";

        public CustomerManagementWindow(CustomerService customerService)
        {
            InitializeComponent();
            Loaded += CustomerManagementWindow_Loaded;

            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        private void CustomerManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                var customerList = customerService.GetAllCustomers();
                customers = new ObservableCollection<Customer>(customerList);
                CustomerDataGrid.ItemsSource = customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCustomerList()
        {
            LoadCustomers();
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerDataGrid.SelectedItem is Customer selectedCustomer)
            {
                // Open customer detail window for editing
                CustomerDetailWindow customerDetailWindow = App.AppHost.Services.GetRequiredService<CustomerDetailWindow>();
                customerDetailWindow.SelectedCustomer = selectedCustomer;
                customerDetailWindow.Account = Account;
                customerDetailWindow.ShowDialog();
                RefreshCustomerList();
            }
            else
            {
                MessageBox.Show("Please select a customer to edit.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerDataGrid.SelectedItem is Customer selectedCustomer)
            {
                var result = MessageBox.Show($"Are you sure you want to delete customer '{selectedCustomer.Name}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        customerService.DeleteCustomer(selectedCustomer.CustomerID);
                        RefreshCustomerList();
                        MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            // Mở form thêm mới khách hàng
            CustomerDetailWindow customerDetailWindow = App.AppHost.Services.GetRequiredService<CustomerDetailWindow>();
            customerDetailWindow.SelectedCustomer = null; // Chế độ thêm mới
            customerDetailWindow.Account = Account;
            customerDetailWindow.ShowDialog();
            RefreshCustomerList();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string keyword = SearchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(keyword) && keyword != searchPlaceholder)
            {
                SearchCustomers(keyword);
            }
            else
            {
                LoadCustomers(); // Show all customers if search is empty
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = searchPlaceholder;
            LoadCustomers();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == searchPlaceholder)
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = searchPlaceholder;
                SearchTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string keyword = SearchTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(keyword) && keyword != searchPlaceholder)
                {
                    SearchCustomers(keyword);
                }
                else
                {
                    LoadCustomers(); // Show all customers if search is empty
                }

                e.Handled = true;
            }
        }

        private void SearchCustomers(string keyword)
        {
            try
            {
                var allCustomers = customerService.GetAllCustomers();
                var filteredCustomers = allCustomers.Where(c =>
                    (c.Name != null && c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Email != null && c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                customers = new ObservableCollection<Customer>(filteredCustomers);
                CustomerDataGrid.ItemsSource = customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}