using FUMiniTikiSystem.BLL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace Group6WPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly CustomerService customerService;
        private readonly string adminEmail;
        private readonly string adminPassword;

        public string email = "alice@example.com";
        public string password = "@1";

        public LoginWindow(CustomerService customerService)
        {
            InitializeComponent();

            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();

            adminEmail = config["AdminAccount:Email"];
            adminPassword = config["AdminAccount:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                MessageBox.Show("Admin account credentials are not set in appsettings.json.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (email == adminEmail && password == adminPassword)
            {
                AdminWindow adminWindow = App.AppHost.Services.GetRequiredService<AdminWindow>();
                adminWindow.Show();
                Close();
                return;
            }

            var account = customerService.Login(email, password);
            if (account == null)
            {
                MessageBox.Show("Invalid email or password. Please try again.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (account.IsActive != null && !account.IsActive.Value)
            {
                MessageBox.Show("Your account is blocked. Please contact support.", "Account Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Account = account;
            mainWindow.Show();

            Close();
        }

        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            SignUpWindow signUpWindow = App.AppHost.Services.GetRequiredService<SignUpWindow>();
            signUpWindow.Show();

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtEmail.Text = email ?? string.Empty;
            txtPassword.Password = password ?? string.Empty;
        }
    }
}
