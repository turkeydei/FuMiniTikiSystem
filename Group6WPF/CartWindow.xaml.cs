using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Group6WPF
{
    public partial class CartWindow : Window
    {
        private readonly CartService cartService;
        private readonly OrderService orderService;
        private readonly CartItemService cartItemService;
        public Cart Cart { get; set; }
        public Customer Account { get; set; }

        public string searchPlaceholder = "Search product by name...";
        public CartWindow(CartItemService cartItemService, CartService cartService, OrderService orderService)
        {
            InitializeComponent();
            this.cartItemService = cartItemService;
            this.orderService = orderService;
            this.cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));

            Loaded += CartWindow_Loaded;
        }

        private void UpdateCartBadge()
        {
            if (Account == null)
            {
                CartBadge.Visibility = Visibility.Collapsed;
                return;
            }

            var cart = cartService.GetCartByCustomerID(Account.CustomerID);
            int totalItems = cart?.CartItems?.Sum(x => x.Quantity) ?? 0;

            if (totalItems > 0)
            {
                CartBadge.Visibility = Visibility.Visible;
                CartCountText.Text = totalItems > 99 ? "99+" : totalItems.ToString();
            }
            else
            {
                CartBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCart()
        {
            if (Account == null)
            {
                MessageBox.Show("Please log in to view your cart.", "Notification",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                UpdateCartBadge(); // Update badge even when not logged in
                return;
            }

            Cart = cartService.GetCartByCustomerID(Account.CustomerID);

            if (Cart?.CartItems == null || !Cart.CartItems.Any())
            {
                ShowEmptyCart();
                UpdateOrderSummary();
                UpdateCartBadge(); // Update badge for empty cart
                return;
            }

            ShowCartItems();
            UpdateOrderSummary();
            UpdateCartBadge(); // Update badge with current item count
        }

        // Update the RefreshCart method
        private void RefreshCart()
        {
            LoadCart();
            UpdateCartBadge();
        }

        // Call UpdateCartBadge in the Loaded event
        private void CartWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCart();
            UpdateCartBadge();
        }
        private void ShowEmptyCart()
        {
            EmptyCartPanel.Visibility = Visibility.Visible;
            CartItemsPanel.Visibility = Visibility.Collapsed;
            CheckoutButton.IsEnabled = false;

            ItemCountTextBlock.Text = "0 items";
            SubtotalTextBlock.Text = "0₫";
        }

        private void ShowCartItems()
        {
            EmptyCartPanel.Visibility = Visibility.Collapsed;
            CartItemsPanel.Visibility = Visibility.Visible;
            CartItemsPanel.Children.Clear();

            foreach (var cartItem in Cart.CartItems)
            {
                var itemCard = CreateCartItemCard(cartItem);
                CartItemsPanel.Children.Add(itemCard);
            }

            ItemCountTextBlock.Text = $"{Cart.CartItems.Sum(x => x.Quantity)} items";
            CheckoutButton.IsEnabled = true;
        }

        private Border CreateCartItemCard(CartItem cartItem)
        {
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(235, 235, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(20)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // Product Image
            var imageContainer = new Border
            {
                Width = 80,
                Height = 80,
                Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                CornerRadius = new CornerRadius(8)
            };

            var productImage = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };

            try
            {
                productImage.Source = new BitmapImage(new Uri(cartItem.Product.Picture, UriKind.Absolute));
            }
            catch
            {
                productImage.Source = new BitmapImage(new Uri("Assets/placeholder.png", UriKind.Relative));
            }

            imageContainer.Child = productImage;
            Grid.SetColumn(imageContainer, 0);

            // Product Info
            var productInfo = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var productName = new TextBlock
            {
                Text = cartItem.Product.Name,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(36, 36, 36)),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 40,
                TextTrimming = TextTrimming.WordEllipsis
            };

            var productDescription = new TextBlock
            {
                Text = cartItem.Product.Description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 137)),
                Margin = new Thickness(0, 5, 0, 0)
            };

            productInfo.Children.Add(productName);
            productInfo.Children.Add(productDescription);
            Grid.SetColumn(productInfo, 1);

            // Price
            var pricePanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var currentPrice = new TextBlock
            {
                Text = $"{cartItem.Product.Price:N0}₫",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 66, 78))
            };

            //var originalPrice = new TextBlock
            //{
            //    Text = $"{cartItem.Product.Price * 1.2m:N0}₫",
            //    FontSize = 12,
            //    Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 137)),
            //    TextDecorations = TextDecorations.Strikethrough,
            //    Margin = new Thickness(0, 2, 0, 0)
            //};

            pricePanel.Children.Add(currentPrice);
            //pricePanel.Children.Add(originalPrice);
            Grid.SetColumn(pricePanel, 2);

            // Quantity Controls
            var quantityPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var decreaseBtn = new Button
            {
                Content = "−",
                Style = (Style)FindResource("QuantityButtonStyle"),
                Tag = cartItem
            };
            decreaseBtn.Click += DecreaseQuantity_Click;

            var quantityText = new TextBox
            {
                Text = cartItem.Quantity.ToString(),
                Width = 50,
                Height = 32,
                TextAlignment = TextAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                BorderBrush = new SolidColorBrush(Color.FromRgb(235, 235, 240)),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = cartItem
            };
            quantityText.TextChanged += QuantityText_TextChanged;

            var increaseBtn = new Button
            {
                Content = "+",
                Style = (Style)FindResource("QuantityButtonStyle"),
                Tag = cartItem
            };
            increaseBtn.Click += IncreaseQuantity_Click;

            quantityPanel.Children.Add(decreaseBtn);
            quantityPanel.Children.Add(quantityText);
            quantityPanel.Children.Add(increaseBtn);
            Grid.SetColumn(quantityPanel, 3);

            // Remove Button
            var removeBtn = new Button
            {
                Content = "🗑️ Remove",
                Style = (Style)FindResource("RemoveButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = cartItem
            };
            removeBtn.Click += RemoveItem_Click;
            Grid.SetColumn(removeBtn, 4);

            grid.Children.Add(imageContainer);
            grid.Children.Add(productInfo);
            grid.Children.Add(pricePanel);
            grid.Children.Add(quantityPanel);
            grid.Children.Add(removeBtn);

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
            decimal total = subtotal;

            SubtotalTextBlock.Text = $"{subtotal:N0}₫";
            TotalTextBlock.Text = $"{total:N0}₫";
        }

        // Event Handlers
        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem cartItem)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    cartItemService.Update(cartItem);
                    RefreshCart();
                }
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem cartItem)
            {
                cartItem.Quantity++;
                cartItemService.Update(cartItem);
                RefreshCart();
            }
        }

        private void QuantityText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is CartItem cartItem)
            {
                if (int.TryParse(textBox.Text, out int quantity) && quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    cartItemService.Update(cartItem);
                    UpdateOrderSummary();
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem cartItem)
            {
                var result = MessageBox.Show("Are you sure you want to remove this item from the cart?",
                                           "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    cartItemService.Delete(cartItem);
                    RefreshCart();
                }
            }
        }

        //private void CheckoutButton_Click(object sender, RoutedEventArgs e)
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

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Cart?.CartItems == null || !Cart.CartItems.Any())
                {
                    MessageBox.Show("Your cart is empty", "Warning",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Navigate to CheckOutWindow instead of placing order directly
                CheckOutWindow checkOutWindow = App.AppHost.Services.GetRequiredService<CheckOutWindow>();
                checkOutWindow.Account = Account;
                checkOutWindow.Cart = Cart;
                checkOutWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to navigate to checkout: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContinueShoppingButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Account = Account;
            mainWindow.Show();
            Close();
        }

        private void btnViewCart_Click(object sender, RoutedEventArgs e)
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            UpdateCartBadge();
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = App.AppHost.Services.GetRequiredService<ProfileWindow>();
            profileWindow.Account = Account;
            profileWindow.Show();
            Close();
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

                if (!string.IsNullOrEmpty(keyword))
                {
                    ProductSearchWindow productSearchWindow = App.AppHost.Services.GetRequiredService<ProductSearchWindow>();
                    productSearchWindow.Account = Account;
                    productSearchWindow.currentSearchTerm = keyword;
                    productSearchWindow.Show();
                    Close();
                }

                e.Handled = true;
            }
        }
    }
}