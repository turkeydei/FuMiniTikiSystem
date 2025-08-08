using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using System.Windows;

namespace Group6WPF
{
    public partial class ViewOrderHistory : Window
    {
        private OrderService orderService;
        private OrderDetailService orderDetailService;
        public Customer Account { get; set; }
        private Order currentOrder;

        public ViewOrderHistory(OrderService orderService, OrderDetailService orderDetailService)
        {
            InitializeComponent();
            this.orderService = orderService;
            this.orderDetailService = orderDetailService;
        }

        public void LoadOrderDetails(Order order)
        {
            if (order == null)
            {
                MessageBox.Show("Invalid order selected.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentOrder = order;

            // Update order information
            OrderIdText.Text = $"#{order.OrderID}";
            OrderDateText.Text = order.OrderDate.ToString("MMMM dd, yyyy 'at' HH:mm");
            OrderStatusText.Text = order.OrderStatus ?? "Pending";
            OrderAmountText.Text = $"{order.OrderAmount:N0}₫";

            // Set status color
            OrderStatusBorder.Background = GetStatusColor(order.OrderStatus);

            try
            {
                // Load order details
                var orderDetails = orderDetailService.GetOrderDetailsByOrderID(order.OrderID);

                if (orderDetails != null && orderDetails.Any())
                {
                    dgOrderDetail.ItemsSource = orderDetails;

                    // Calculate totals
                    var subtotal = orderDetails.Sum(od => od.Quantity * od.UnitPrice);
                    var totalItems = orderDetails.Sum(od => od.Quantity);

                    SubtotalText.Text = $"{subtotal:N0}₫";
                    TotalItemsText.Text = $"{totalItems} item(s)";
                }
                else
                {
                    dgOrderDetail.ItemsSource = null;
                    SubtotalText.Text = "0₫";
                    TotalItemsText.Text = "0 items";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load order details: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Windows.Media.Brush GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green),
                "pending" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange),
                "cancelled" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                "processing" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue),
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray)
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BackToProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}