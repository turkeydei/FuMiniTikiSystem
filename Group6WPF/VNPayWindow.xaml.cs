using FUMiniTikiSystem.BLL;
using Microsoft.Web.WebView2.Core;
using System.Windows;

namespace Group6WPF
{
    public partial class VNPayWindow : Window
    {
        private readonly VNPAYService _vnpayService;
        public decimal Amount { get; set; }
        public int OrderId { get; set; }
        public int PaymentId { get; set; }
        public bool PaymentSuccessful { get; private set; }

        public VNPayWindow(VNPAYService vnpayService)
        {
            InitializeComponent();
            _vnpayService = vnpayService ?? throw new ArgumentNullException(nameof(vnpayService));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeWebView();
            LoadVNPayPayment();
        }

        private async Task InitializeWebView()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);

                // Handle navigation events
                webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                webView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadVNPayPayment()
        {
            try
            {
                string paymentUrl = _vnpayService.CreatePaymentUrl(Amount, OrderId, PaymentId);
                webView.CoreWebView2.Navigate(paymentUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating payment URL: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Check if this is a return URL from VNPay
            if (e.Uri.StartsWith("http://localhost:8888/vnpay_return", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                ProcessPaymentReturn(e.Uri);
            }
        }

        private void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void ProcessPaymentReturn(string returnUrl)
        {
            try
            {
                var uri = new Uri(returnUrl);

                PaymentSuccessful = _vnpayService.ValidatePaymentResponse(uri.Query);

                DialogResult = PaymentSuccessful;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment return: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                PaymentSuccessful = false;
                DialogResult = false;
                Close();
            }
        }
        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentSuccessful = false;
            DialogResult = false;
            Close();
        }
    }
}