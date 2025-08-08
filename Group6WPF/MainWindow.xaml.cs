using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.BLL.DTOs;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Group6WPF
{
    public partial class MainWindow : Window
    {
        private readonly ProductService productService;
        private readonly CategoryService categoryService;
        private readonly CartService cartService;

        // Pagination properties
        private int currentPage = 1;
        private int pageSize = 8; // 4 columns × 2 rows = 8 products per page
        private int totalPages = 1;

        public Customer Account { get; set; }
        public string searchPlaceholder = "Search product by name...";
        public MainWindow(ProductService productService, CategoryService categoryService, CartService cartService)
        {
            InitializeComponent();

            this.productService = productService ?? throw new ArgumentNullException(nameof(productService));
            this.categoryService = categoryService;
            this.cartService = cartService;

            LoadProducts();
            LoadCategories();
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

        private void LoadCategories()
        {
            var categories = categoryService.GetAllCategories();
            CategoryListBox.SelectionChanged -= CategoryListBox_SelectionChanged; // Remove previous handler
            CategoryListBox.ItemsSource = categories;
            CategoryListBox.SelectionChanged += CategoryListBox_SelectionChanged; // Add handler
        }

        private void LoadProducts()
        {
            var request = new GetProductsRequest
            {
                PageNumber = currentPage,
                PageSize = pageSize,
                IsActive = true,
                OrderBy = "trend", // Fixed to trend ordering
                SortDirection = SortDirection.Desc // Always descending for most popular first
            };

            var paginatedProducts = productService.GetAllProducts(request);
            totalPages = paginatedProducts.TotalPages;

            ProductGrid.Children.Clear();

            foreach (var product in paginatedProducts.Items)
            {
                CreateProductCard(product);
            }

            UpdatePaginationControls();
        }

        private void CreateProductCard(Product product)
        {
            // Main product card border with shadow effect
            var productCard = new Border
            {
                Width = 200,
                Height = 250,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Cursor = System.Windows.Input.Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 315,
                    ShadowDepth = 3,
                    Opacity = 0.2,
                    BlurRadius = 8
                }
            };

            // Content container
            var productContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(12)
            };

            // Product image container
            var imageContainer = new Border
            {
                Height = 140,
                Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var productImage = new Image
            {
                Height = 130,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

            try
            {
                productImage.Source = new BitmapImage(new Uri(product.Picture, UriKind.Absolute));
            }
            catch
            {
                productImage.Source = new BitmapImage(new Uri("Assets/placeholder.png", UriKind.Relative));
            }

            imageContainer.Child = productImage;

            // Product name
            var productName = new TextBlock
            {
                Text = product.Name,
                FontWeight = FontWeights.Medium,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                FontSize = 14,
                MaxHeight = 42,
                LineHeight = 18,
                Margin = new Thickness(0, 0, 0, 8),
                TextTrimming = TextTrimming.WordEllipsis
            };

            // Price container
            var priceContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var productPrice = new TextBlock
            {
                Text = $"{product.Price:N0}đ",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 66, 78)),
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };

            priceContainer.Children.Add(productPrice);

            // Assemble the card content
            productContent.Children.Add(imageContainer);
            productContent.Children.Add(productName);
            productContent.Children.Add(priceContainer);

            productCard.Child = productContent;

            // Create the clickable button that contains the entire card
            var clickableButton = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Content = productCard,
                Tag = product,
                Padding = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Template = new ControlTemplate(typeof(Button))
                {
                    VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
                }
            };

            // Enhanced hover effects for the entire card
            clickableButton.MouseEnter += (s, e) =>
            {
                productCard.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 315,
                    ShadowDepth = 6,
                    Opacity = 0.35,
                    BlurRadius = 15
                };

                var scaleTransform = new ScaleTransform(1.02, 1.02);
                productCard.RenderTransform = scaleTransform;
                productCard.RenderTransformOrigin = new Point(0.5, 0.5);
            };

            clickableButton.MouseLeave += (s, e) =>
            {
                productCard.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 315,
                    ShadowDepth = 3,
                    Opacity = 0.2,
                    BlurRadius = 8
                };

                productCard.RenderTransform = Transform.Identity;
            };

            clickableButton.Click += ProductButton_Click;
            ProductGrid.Children.Add(clickableButton);
        }

        private void UpdatePaginationControls()
        {
            txtPageInfo.Text = $"Page {currentPage} of {totalPages}";
            btnPrevious.IsEnabled = currentPage > 1;
            btnNext.IsEnabled = currentPage < totalPages;
            btnPrevious.Opacity = btnPrevious.IsEnabled ? 1.0 : 0.5;
            btnNext.Opacity = btnNext.IsEnabled ? 1.0 : 0.5;
        }

        // Event Handlers
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadProducts();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadProducts();
            }
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

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is Category selectedCategory)
            {
                CategoryMainWindow categoryWindow = App.AppHost.Services.GetRequiredService<CategoryMainWindow>();
                categoryWindow.SelectedCategory = selectedCategory;
                categoryWindow.Account = Account;
                categoryWindow.Show();
                Close();
            }
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product selectedProduct)
            {
                ProductMainWindow productWindow = App.AppHost.Services.GetRequiredService<ProductMainWindow>();
                productWindow.SelectedProduct = selectedProduct;
                productWindow.Account = Account;
                productWindow.Show();
                Close();
            }
        }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCartBadge();
            LoadProducts();
            LoadCategories();
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = App.AppHost.Services.GetRequiredService<ProfileWindow>();
            profileWindow.Account = Account;
            profileWindow.Show();
            Close();
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
        private async void SendAIMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageToAIAsync();
        }

        private async void AiMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SendMessageToAIAsync();
            }
        }
        // Add these methods to your MainWindow.xaml.cs class

        private bool isChatMinimized = false;
        private string chatPlaceholder = "Type a message...";

        // Chat bubble click to expand
        private void ChatBubble_Click(object sender, MouseButtonEventArgs e)
        {
            ExpandChat();
        }

        // Minimize button click
        private void MinimizeChat_Click(object sender, RoutedEventArgs e)
        {
            MinimizeChat();
        }

        // Expand chat method
        private void ExpandChat()
        {
            ExpandedChatBox.Visibility = Visibility.Visible;
            MinimizedChatBubble.Visibility = Visibility.Collapsed;
            isChatMinimized = false;

            // Hide notification badge when chat is opened
            NotificationBadge.Visibility = Visibility.Collapsed;

            // Focus on text box
            AiMessageTextBox.Focus();

            // Scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        // Minimize chat method
        private void MinimizeChat()
        {
            ExpandedChatBox.Visibility = Visibility.Collapsed;
            MinimizedChatBubble.Visibility = Visibility.Visible;
            isChatMinimized = true;
        }

        // Enhanced message input handling
        private void AiMessageTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AiMessageTextBox.Text == chatPlaceholder)
            {
                AiMessageTextBox.Text = "";
                AiMessageTextBox.Foreground = Brushes.Black;
            }
        }

        private void AiMessageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AiMessageTextBox.Text))
            {
                AiMessageTextBox.Text = chatPlaceholder;
                AiMessageTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            }
        }

        // Enhanced SendMessageToAIAsync method with better UI
        private async Task SendMessageToAIAsync()
        {
            string userMessage = AiMessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userMessage) || userMessage == chatPlaceholder) return;

            // Clear input
            AiMessageTextBox.Text = "";
            AiMessageTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));

            // Add user message bubble
            AddUserMessage(userMessage);

            // Add typing indicator
            var typingIndicator = AddTypingIndicator();

            try
            {
                using var client = new HttpClient();
                var payload = new { user_message = userMessage };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://127.0.0.1:8004/ask", content);

                // Remove typing indicator
                ChatHistoryPanel.Children.Remove(typingIndicator);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    string aiReply = jsonDoc.RootElement.GetProperty("response").GetString();

                    AddAIMessage(aiReply);
                }
                else
                {
                    AddAIMessage("Sorry, I couldn't process your request right now. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                // Remove typing indicator if still present
                if (ChatHistoryPanel.Children.Contains(typingIndicator))
                    ChatHistoryPanel.Children.Remove(typingIndicator);

                AddAIMessage("Sorry, there was an error connecting to the AI service.");
            }

            // Show notification if chat is minimized
            if (isChatMinimized)
            {
                ShowNotification();
            }

            // Auto-scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        // Add user message bubble
        private void AddUserMessage(string message)
        {
            var userBubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 66, 78)),
                CornerRadius = new CornerRadius(15, 15, 5, 15),
                Padding = new Thickness(12),
                Margin = new Thickness(50, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 3,
                    ShadowDepth = 1,
                    Opacity = 0.1
                }
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = Brushes.White
            };

            userBubble.Child = textBlock;
            ChatHistoryPanel.Children.Add(userBubble);
        }

        // Add AI message bubble
        private void AddAIMessage(string message)
        {
            var aiBubble = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(15, 15, 15, 5),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 5, 50, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 3,
                    ShadowDepth = 1,
                    Opacity = 0.1
                }
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
            };

            aiBubble.Child = textBlock;
            ChatHistoryPanel.Children.Add(aiBubble);
        }

        // Add typing indicator
        private Border AddTypingIndicator()
        {
            var typingBubble = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(15, 15, 15, 5),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 5, 50, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 3,
                    ShadowDepth = 1,
                    Opacity = 0.1
                }
            };

            var textBlock = new TextBlock
            {
                Text = "AI is typing...",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                FontStyle = FontStyles.Italic
            };

            typingBubble.Child = textBlock;
            ChatHistoryPanel.Children.Add(typingBubble);
            ChatScrollViewer.ScrollToBottom();

            return typingBubble;
        }

        // Show notification when chat is minimized
        private void ShowNotification()
        {
            NotificationBadge.Visibility = Visibility.Visible;
            NotificationCount.Text = "1";
        }

    }
}