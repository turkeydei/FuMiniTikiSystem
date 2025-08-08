using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Group6WPF
{
    /// <summary>
    /// Interaction logic for SignUpWindow.xaml
    /// </summary>
    public partial class SignUpWindow : Window
    {
        private readonly CustomerService customerService;

        public SignUpWindow(CustomerService customerService)
        {
            InitializeComponent();

            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Password) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Please fill in all fields.", "Sign Up Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Passwords do not match.", "Sign Up Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newCustomer = new Customer
            {
                Name = txtName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Password.Trim()
            };

            var context = new ValidationContext(newCustomer, null, null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(newCustomer, context, results, true);

            if (!isValid)
            {
                string errors = string.Join("\n", results.Select(r => r.ErrorMessage));
                MessageBox.Show($"Sign up failed:\n{errors}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingCustomer = customerService.GetCustomerByEmail(newCustomer.Email);
            if (existingCustomer != null)
            {
                MessageBox.Show("Email already exists. Please use a different email.", "Sign Up Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                customerService.AddCustomer(newCustomer);
                MessageBox.Show("Sign up successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoginWindow loginWindow = App.AppHost.Services.GetRequiredService<LoginWindow>();
                loginWindow.email = newCustomer.Email;
                loginWindow.password = newCustomer.Password;
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sign up failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
