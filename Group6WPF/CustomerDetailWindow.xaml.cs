using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using System.Text.RegularExpressions;
using System.Windows;

namespace Group6WPF
{
    /// <summary>
    /// Interaction logic for CustomerDetailWindow.xaml
    /// </summary>
    public partial class CustomerDetailWindow : Window
    {
        private readonly CustomerService customerService;
        public Customer SelectedCustomer { get; set; }
        public Customer Account { get; set; }

        public CustomerDetailWindow(CustomerService customerService)
        {
            InitializeComponent();
            Loaded += CustomerDetailWindow_Loaded;

            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        private void CustomerDetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer != null)
            {
                // Edit mode - populate fields with existing data
                TitleTextBlock.Text = "Edit Customer";
                CustomerIDTextBox.Text = SelectedCustomer.CustomerID.ToString();
                NameTextBox.Text = SelectedCustomer.Name ?? "";
                EmailTextBox.Text = SelectedCustomer.Email;
                PasswordBox.Password = SelectedCustomer.Password;
                ConfirmPasswordBox.Password = SelectedCustomer.Password;

                // Set status
                if (SelectedCustomer.IsActive == true)
                {
                    StatusComboBox.SelectedIndex = 0; // Active
                }
                else
                {
                    StatusComboBox.SelectedIndex = 1; // Inactive
                }
                CustomerIDTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                // Add mode
                TitleTextBlock.Text = "Add Customer";
                CustomerIDTextBox.Visibility = Visibility.Collapsed;
                NameTextBox.Text = "";
                EmailTextBox.Text = "";
                PasswordBox.Password = "";
                ConfirmPasswordBox.Password = "";
                StatusComboBox.SelectedIndex = 0;
            }
        }

        private bool ValidateInputs()
        {
            // Validate Name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter customer name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            // Validate email format
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(EmailTextBox.Text, emailPattern))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }



            // Validate Password
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            // Validate Confirm Password
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            if (SelectedCustomer != null)
            {
                // Update customer
                SelectedCustomer.Name = NameTextBox.Text.Trim();
                SelectedCustomer.Email = EmailTextBox.Text.Trim();
                SelectedCustomer.Password = PasswordBox.Password;
                SelectedCustomer.IsActive = (StatusComboBox.SelectedIndex == 0);
                try
                {
                    customerService.UpdateCustomer(SelectedCustomer);
                    MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Add new customer
                var newCustomer = new Customer
                {
                    Name = NameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Password = PasswordBox.Password,
                    IsActive = (StatusComboBox.SelectedIndex == 0)
                };
                try
                {
                    customerService.AddCustomer(newCustomer);
                    MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog
            Close();
        }
    }
}