using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Group6WPF
{
    public partial class ProfileWindow : Window
    {
        private readonly CustomerService customerService;
        private readonly CartService cartService;
        private readonly OrderService orderService;
        public Customer Account { get; set; }
        private Customer originalCustomer;

        public string searchPlaceholder = "Search product by name...";
        public ProfileWindow(CustomerService customerService, CartService cartService, OrderService orderService)
        {
            InitializeComponent();
            this.customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            this.cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserProfile();
            UpdateCartBadge();
            ShowProfileInfo();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Account = Account;
            mainWindow.Show();
            Close();
        }

        private void LoadUserProfile()
        {
            if (Account == null)
            {
                MessageBox.Show("Please log in to view your profile.", "Authentication Required",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            // Create a copy of the original customer data
            originalCustomer = new Customer
            {
                CustomerID = Account.CustomerID,
                Name = Account.Name,
                Email = Account.Email,
                Password = Account.Password,
                IsActive = Account.IsActive
            };

            // Load data into form
            ProfileNameText.Text = string.IsNullOrEmpty(Account.Name) ? Account.Email : Account.Name;
            NameTextBox.Text = Account.Name ?? "";
            EmailTextBox.Text = Account.Email;
            IsActiveCheckBox.IsChecked = Account.IsActive ?? true;
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

        #region Tab Navigation
        private void ProfileInfoTab_Click(object sender, RoutedEventArgs e)
        {
            ShowProfileInfo();
        }

        private void OrderHistoryTab_Click(object sender, RoutedEventArgs e)
        {
            ShowOrderHistory();
        }

        private void ShowProfileInfo()
        {
            ProfileInfoPanel.Visibility = Visibility.Visible;
            OrderHistoryPanel.Visibility = Visibility.Collapsed;

            // Update tab button styles
            ProfileInfoTab.Background = new SolidColorBrush(Color.FromRgb(248, 248, 255));
            ProfileInfoTab.Foreground = FindResource("TikiBlue") as SolidColorBrush;
            OrderHistoryTab.Background = Brushes.Transparent;
            OrderHistoryTab.Foreground = FindResource("TextSecondary") as SolidColorBrush;
        }

        private void ShowOrderHistory()
        {
            ProfileInfoPanel.Visibility = Visibility.Collapsed;
            OrderHistoryPanel.Visibility = Visibility.Visible;

            // Update tab button styles
            OrderHistoryTab.Background = new SolidColorBrush(Color.FromRgb(248, 248, 255));
            OrderHistoryTab.Foreground = FindResource("TikiBlue") as SolidColorBrush;
            ProfileInfoTab.Background = Brushes.Transparent;
            ProfileInfoTab.Foreground = FindResource("TextSecondary") as SolidColorBrush;

            LoadOrderHistory();
        }
        #endregion

        #region Profile Management
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Verify current password if user wants to change password
                if (!string.IsNullOrEmpty(NewPasswordBox.Password))
                {
                    if (string.IsNullOrEmpty(CurrentPasswordBox.Password))
                    {
                        ShowError("Please enter your current password to change your password.");
                        return;
                    }

                    if (CurrentPasswordBox.Password != Account.Password)
                    {
                        ShowError("Current password is incorrect.");
                        return;
                    }

                    if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        ShowError("New passwords do not match.");
                        return;
                    }

                    Account.Password = NewPasswordBox.Password;
                }

                // Update other fields
                Account.Name = NameTextBox.Text.Trim();
                Account.Email = EmailTextBox.Text.Trim();
                Account.IsActive = IsActiveCheckBox.IsChecked;

                // Save changes
                customerService.UpdateCustomer(Account);

                MessageBox.Show("Profile updated successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear password fields
                CurrentPasswordBox.Clear();
                NewPasswordBox.Clear();
                ConfirmPasswordBox.Clear();

                // Update the display
                ProfileNameText.Text = string.IsNullOrEmpty(Account.Name) ? Account.Email : Account.Name;
                HideError();

                // Update original customer data
                originalCustomer.Name = Account.Name;
                originalCustomer.Email = Account.Email;
                originalCustomer.Password = Account.Password;
                originalCustomer.IsActive = Account.IsActive;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update profile: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore original values
            NameTextBox.Text = originalCustomer.Name ?? "";
            EmailTextBox.Text = originalCustomer.Email;
            IsActiveCheckBox.IsChecked = originalCustomer.IsActive ?? true;

            // Clear password fields
            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();

            HideError();
        }

        private bool ValidateInput()
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowError("Name is required.");
                NameTextBox.Focus();
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowError("Email is required.");
                EmailTextBox.Focus();
                return false;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowError("Please enter a valid email address.");
                EmailTextBox.Focus();
                return false;
            }

            // Check if email is already taken by another user
            if (EmailTextBox.Text.Trim() != originalCustomer.Email)
            {
                var existingCustomer = customerService.GetCustomerByEmail(EmailTextBox.Text.Trim());
                if (existingCustomer != null && existingCustomer.CustomerID != Account.CustomerID)
                {
                    ShowError("This email address is already registered to another account.");
                    EmailTextBox.Focus();
                    return false;
                }
            }

            // Validate password if changing
            if (!string.IsNullOrEmpty(NewPasswordBox.Password))
            {
                if (NewPasswordBox.Password.Length < 6)
                {
                    ShowError("New password must be at least 6 characters long.");
                    NewPasswordBox.Focus();
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Order History
        private void LoadOrderHistory()
        {
            try
            {
                var orders = orderService.GetCustomerOrders(Account.CustomerID);

                // Update statistics
                TotalOrdersText.Text = orders.Count.ToString();
                CompletedOrdersText.Text = orders.Count(o => o.OrderStatus == "Completed").ToString();
                TotalSpentText.Text = $"{orders.Sum(o => o.OrderAmount):N0}₫";

                // Clear existing orders
                OrdersListPanel.Children.Clear();

                if (!orders.Any())
                {
                    var emptyMessage = new TextBlock
                    {
                        Text = "You haven't placed any orders yet.",
                        FontSize = 16,
                        Foreground = FindResource("TextSecondary") as SolidColorBrush,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    };
                    OrdersListPanel.Children.Add(emptyMessage);
                    return;
                }

                // Add order cards
                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                {
                    var orderCard = CreateOrderCard(order);
                    OrdersListPanel.Children.Add(orderCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load order history: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private Border CreateOrderCard(Order order)
        //{
        //    var card = new Border
        //    {
        //        Background = Brushes.White,
        //        BorderBrush = FindResource("BorderColor") as SolidColorBrush,
        //        BorderThickness = new Thickness(1),
        //        CornerRadius = new CornerRadius(8),
        //        Margin = new Thickness(0, 0, 0, 15),
        //        Padding = new Thickness(20)
        //    };

        //    var grid = new Grid();
        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        //    // Order info
        //    var orderInfo = new StackPanel();

        //    var orderNumber = new TextBlock
        //    {
        //        Text = $"Order #{order.OrderID}",
        //        FontSize = 16,
        //        FontWeight = FontWeights.SemiBold,
        //        Foreground = FindResource("TextPrimary") as SolidColorBrush
        //    };

        //    var orderDate = new TextBlock
        //    {
        //        Text = $"Placed on {order.OrderDate:MMM dd, yyyy}",
        //        FontSize = 12,
        //        Foreground = FindResource("TextSecondary") as SolidColorBrush,
        //        Margin = new Thickness(0, 4, 0, 0)
        //    };

        //    var itemCount = new TextBlock
        //    {
        //        Text = $"{order.OrderDetails?.Count ?? 0} items",
        //        FontSize = 12,
        //        Foreground = FindResource("TextSecondary") as SolidColorBrush,
        //        Margin = new Thickness(0, 2, 0, 0)
        //    };

        //    orderInfo.Children.Add(orderNumber);
        //    orderInfo.Children.Add(orderDate);
        //    orderInfo.Children.Add(itemCount);
        //    Grid.SetColumn(orderInfo, 0);

        //    // Status badge
        //    var statusBorder = new Border
        //    {
        //        Background = GetStatusColor(order.OrderStatus),
        //        CornerRadius = new CornerRadius(12),
        //        Padding = new Thickness(6, 6, 6, 6),
        //        VerticalAlignment = VerticalAlignment.Top
        //    };

        //    var statusText = new TextBlock
        //    {
        //        Text = order.OrderStatus ?? "Pending",
        //        FontSize = 11,
        //        FontWeight = FontWeights.SemiBold,
        //        Foreground = Brushes.White
        //    };

        //    statusBorder.Child = statusText;
        //    Grid.SetColumn(statusBorder, 1);

        //    // Total amount
        //    // Total amount (continued)
        //    var totalText = new TextBlock
        //    {
        //        Text = $"{order.OrderAmount:N0}₫",
        //        FontSize = 16,
        //        FontWeight = FontWeights.Bold,
        //        Foreground = FindResource("TikiRed") as SolidColorBrush,
        //        VerticalAlignment = VerticalAlignment.Center,
        //        Margin = new Thickness(20, 0, 0, 0)
        //    };
        //    Grid.SetColumn(totalText, 2);

        //    grid.Children.Add(orderInfo);
        //    grid.Children.Add(statusBorder);
        //    grid.Children.Add(totalText);
        //    card.Child = grid;

        //    return card;
        //}
        private void OrderCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag is Order selectedOrder)
            {
                try
                {
                    // Create and show ViewOrderHistory window
                    ViewOrderHistory orderHistoryWindow = App.AppHost.Services.GetRequiredService<ViewOrderHistory>();
                    orderHistoryWindow.Account = Account;
                    orderHistoryWindow.LoadOrderDetails(selectedOrder);
                    orderHistoryWindow.ShowDialog(); // Use ShowDialog to make it modal
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open order details: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private Border CreateOrderCard(Order order)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = FindResource("BorderColor") as SolidColorBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(20),
                Cursor = Cursors.Hand, // Add cursor to indicate clickable
                Tag = order // Store the order object for click event
            };

            // Add click event handler
            card.MouseLeftButtonUp += OrderCard_Click;

            // Add hover effect
            card.MouseEnter += (s, e) =>
            {
                card.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            };

            card.MouseLeave += (s, e) =>
            {
                card.Background = Brushes.White;
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Order info
            var orderInfo = new StackPanel();

            var orderNumber = new TextBlock
            {
                Text = $"Order #{order.OrderID}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = FindResource("TextPrimary") as SolidColorBrush
            };

            var orderDate = new TextBlock
            {
                Text = $"Placed on {order.OrderDate:MMM dd, yyyy}",
                FontSize = 12,
                Foreground = FindResource("TextSecondary") as SolidColorBrush,
                Margin = new Thickness(0, 4, 0, 0)
            };

            var itemCount = new TextBlock
            {
                Text = $"{order.OrderDetails?.Count ?? 0} items",
                FontSize = 12,
                Foreground = FindResource("TextSecondary") as SolidColorBrush,
                Margin = new Thickness(0, 2, 0, 0)
            };

            // Add click hint
            var clickHint = new TextBlock
            {
                Text = "Click to view details →",
                FontSize = 10,
                Foreground = FindResource("TikiBlue") as SolidColorBrush,
                Margin = new Thickness(0, 8, 0, 0),
                FontStyle = FontStyles.Italic
            };

            orderInfo.Children.Add(orderNumber);
            orderInfo.Children.Add(orderDate);
            orderInfo.Children.Add(itemCount);
            orderInfo.Children.Add(clickHint);
            Grid.SetColumn(orderInfo, 0);

            // Status badge
            var statusBorder = new Border
            {
                Background = GetStatusColor(order.OrderStatus),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(6),
                VerticalAlignment = VerticalAlignment.Top
            };

            var statusText = new TextBlock
            {
                Text = order.OrderStatus ?? "Pending",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            statusBorder.Child = statusText;
            Grid.SetColumn(statusBorder, 1);

            // Total amount
            var totalText = new TextBlock
            {
                Text = $"{order.OrderAmount:N0}₫",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = FindResource("TikiRed") as SolidColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };
            Grid.SetColumn(totalText, 2);

            grid.Children.Add(orderInfo);
            grid.Children.Add(statusBorder);
            grid.Children.Add(totalText);
            card.Child = grid;

            return card;
        }
        private SolidColorBrush GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "completed" => FindResource("TikiGreen") as SolidColorBrush ?? new SolidColorBrush(Colors.Green),
                "pending" => FindResource("TikiOrange") as SolidColorBrush ?? new SolidColorBrush(Colors.Orange),
                "cancelled" => FindResource("TikiRed") as SolidColorBrush ?? new SolidColorBrush(Colors.Red),
                "processing" => FindResource("TikiBlue") as SolidColorBrush ?? new SolidColorBrush(Colors.Blue),
                _ => FindResource("TextSecondary") as SolidColorBrush ?? new SolidColorBrush(Colors.Gray)
            };
        }
        #endregion

        #region Navigation
        private void LogoButton_Click(object sender, RoutedEventArgs e)
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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = App.AppHost.Services.GetRequiredService<LoginWindow>();
                loginWindow.email = Account.Email;
                loginWindow.password = Account.Password;
                loginWindow.Show();
                Close();
            }
        }
        #endregion

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