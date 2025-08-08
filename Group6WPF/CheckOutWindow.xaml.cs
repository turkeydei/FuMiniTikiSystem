using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Group6WPF
{
    public partial class CheckOutWindow : Window
    {
        private readonly CartService cartService;
        private readonly OrderService orderService;
        private readonly PaymentService paymentService;
        private readonly CartItemService cartItemService;
        public Cart Cart { get; set; }
        public Customer Account { get; set; }

        public CheckOutWindow(CartItemService cartItemService, CartService cartService, OrderService orderService, PaymentService paymentService)
        {
            InitializeComponent();
            this.cartItemService = cartItemService;
            this.orderService = orderService;
            this.paymentService = paymentService;
            this.cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCustomerInformation();
            LoadOrderItems();
            UpdateOrderSummary();
        }

        private void LoadCustomerInformation()
        {
            if (Account != null)
            {
                CustomerIDText.Text = Account.CustomerID.ToString() ?? "N/A";
                CustomerNameText.Text = Account.Name ?? "N/A";
                CustomerEmailText.Text = Account.Email ?? "N/A";
            }
        }

        private void LoadOrderItems()
        {
            if (Cart?.CartItems == null || !Cart.CartItems.Any())
                return;

            OrderItemsPanel.Children.Clear();

            foreach (var cartItem in Cart.CartItems)
            {
                var itemCard = CreateOrderItemCard(cartItem);
                OrderItemsPanel.Children.Add(itemCard);
            }
        }

        private Border CreateOrderItemCard(CartItem cartItem)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(235, 235, 240)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 15, 0, 15)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            // Product Image
            var productImage = new Image
            {
                Width = 50,
                Height = 50,
                Stretch = Stretch.Uniform
            };

            try
            {
                productImage.Source = new BitmapImage(new Uri(cartItem.Product.Picture, UriKind.Absolute));
            }
            catch
            {
                productImage.Source = new BitmapImage(new Uri("Assets/placeholder.png", UriKind.Relative));
            }

            Grid.SetColumn(productImage, 0);

            // Product Info
            var productInfo = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var productName = new TextBlock
            {
                Text = cartItem.Product.Name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 36, 36)),
                TextWrapping = TextWrapping.Wrap
            };

            productInfo.Children.Add(productName);
            Grid.SetColumn(productInfo, 1);

            // Quantity
            var quantityText = new TextBlock
            {
                Text = $"x{cartItem.Quantity}",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 137))
            };
            Grid.SetColumn(quantityText, 2);

            // Price
            var priceText = new TextBlock
            {
                Text = $"{cartItem.Product.Price * cartItem.Quantity:N0}₫",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 66, 78))
            };
            Grid.SetColumn(priceText, 3);

            grid.Children.Add(productImage);
            grid.Children.Add(productInfo);
            grid.Children.Add(quantityText);
            grid.Children.Add(priceText);

            card.Child = grid;
            return card;
        }

        private void UpdateOrderSummary()
        {
            if (Cart?.CartItems == null || !Cart.CartItems.Any())
            {
                SubtotalTextBlock.Text = "0₫";
                TotalTextBlock.Text = "0₫";
                return;
            }

            decimal subtotal = Cart.CartItems.Sum(x => x.Product.Price * (x.Quantity ?? 1));
            decimal total = subtotal; // Free shipping

            SubtotalTextBlock.Text = $"{subtotal:N0}₫";
            TotalTextBlock.Text = $"{total:N0}₫";
        }

        //private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (Cart?.CartItems == null || !Cart.CartItems.Any())
        //        {
        //            MessageBox.Show("Your cart is empty", "Warning",
        //                          MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return;
        //        }

        //        // Calculate total amount from cart items
        //        decimal totalAmount = Cart.CartItems.Sum(x => x.Product.Price * (x.Quantity ?? 1));

        //        // Create order with all cart items
        //        var order = new Order()
        //        {
        //            CustomerID = Account.CustomerID,
        //            OrderAmount = totalAmount,
        //            OrderStatus = "Pending",
        //            OrderDetails = Cart.CartItems.Select(cartItem => new OrderDetail
        //            {
        //                ProductID = cartItem.Product.ProductID,
        //                Quantity = cartItem.Quantity ?? 1,
        //                UnitPrice = cartItem.Product.Price
        //            }).ToList()
        //        };

        //        // Add the order
        //        orderService.AddOrder(order);

        //        // Clear the cart after successful order
        //        foreach (var cartItem in Cart.CartItems.ToList())
        //        {
        //            cartItemService.Delete(cartItem);
        //        }

        //        MessageBox.Show("Order placed successfully!", "Success",
        //                       MessageBoxButton.OK, MessageBoxImage.Information);

        //        // Navigate back to main window
        //        MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        //        mainWindow.Account = Account;
        //        mainWindow.Show();
        //        Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Failed to place order: {ex.Message}", "Error",
        //                       MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Cart?.CartItems == null || !Cart.CartItems.Any())
                {
                    MessageBox.Show("Your cart is empty", "Warning",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Calculate total amount from cart items
                decimal totalAmount = Cart.CartItems.Sum(x => x.Product.Price * (x.Quantity ?? 1));

                var order = new Order()
                {
                    CustomerID = Account.CustomerID,
                    OrderAmount = totalAmount,
                    OrderStatus = "Pending",
                    OrderDetails = Cart.CartItems.Select(cartItem => new OrderDetail
                    {
                        ProductID = cartItem.Product.ProductID,
                        Quantity = cartItem.Quantity ?? 1,
                        UnitPrice = cartItem.Product.Price
                    }).ToList()
                };

                // Add the order first to get the OrderID
                orderService.AddOrder(order);

                var payment = new Payment()
                {
                    OrderID = order.OrderID,
                    PaymentAmount = totalAmount,
                    PaymentMethod = "VNPay",
                    PaymentStatus = "Pending",
                };

                paymentService.AddPayment(payment);

                // Open VNPay payment window
                VNPayWindow vnpayWindow = App.AppHost.Services.GetRequiredService<VNPayWindow>();
                vnpayWindow.Amount = totalAmount;
                vnpayWindow.OrderId = order.OrderID;
                vnpayWindow.PaymentId = payment.PaymentID;

                bool? paymentResult = vnpayWindow.ShowDialog();

                if (paymentResult == true && vnpayWindow.PaymentSuccessful)
                {
                    MessageBox.Show("Payment and order completed successfully!", "Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    // Navigate back to main window
                    MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
                    mainWindow.Account = Account;
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Payment was cancelled or failed.", "Payment Cancelled",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to process payment: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BackToCartButton_Click(object sender, RoutedEventArgs e)
        {
            CartWindow cartWindow = App.AppHost.Services.GetRequiredService<CartWindow>();
            cartWindow.Account = Account;
            cartWindow.Show();
            Close();
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Account = Account;
            mainWindow.Show();
            Close();
        }
    }
}