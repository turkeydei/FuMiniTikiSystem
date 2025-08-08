using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.BLL.DTOs;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Group6WPF
{
    public partial class CategoryMainWindow : Window
    {
        private readonly ProductService productService;
        private readonly CartService cartService;

        // Pagination properties
        private int currentPage = 1;
        private int pageSize = 8; // 4 columns × 2 rows = 8 products per page
        private int totalPages = 1;
        private string currentSearchTerm = "";
        private string currentOrderBy = "trend";
        public string searchPlaceholder = "Search product by name...";

        // Price filter properties
        private decimal minPrice = 0;
        private decimal maxPrice = 10000000;
        private DispatcherTimer priceFilterTimer;

        public Customer Account { get; set; }
        public Category SelectedCategory { get; set; }

        public CategoryMainWindow(ProductService productService, CartService cartService)
        {
            InitializeComponent();
            this.productService = productService;
            this.cartService = cartService;

            // Initialize timer for delayed filtering - Fixed initialization
            InitializePriceFilterTimer();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Account = Account;
            mainWindow.Show();
            Close();
        }
        private void InitializePriceFilterTimer()
        {
            priceFilterTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(800) // 800ms delay to avoid excessive calls
            };
            priceFilterTimer.Tick += PriceFilterTimer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedCategory != null)
            {
                LoadCategoryInfo();
                LoadProducts();
                UpdateCartBadge();
            }
        }

        private void LoadCategoryInfo()
        {
            CategoryName.Text = SelectedCategory.Name;
            CategoryDescription.Text = SelectedCategory.Description ?? "";

            try
            {
                if (!string.IsNullOrEmpty(SelectedCategory.Picture))
                {
                    CategoryImage.Source = new BitmapImage(new Uri(SelectedCategory.Picture, UriKind.Absolute));
                }
            }
            catch
            {
                CategoryImage.Source = new BitmapImage(new Uri("Assets/placeholder.png", UriKind.Relative));
            }
        }

        private void LoadProducts()
        {
            if (SelectedCategory != null)
            {
                var request = new GetProductsRequest
                {
                    PageNumber = currentPage,
                    PageSize = pageSize,
                    SearchTerm = currentSearchTerm,
                    CategoryID = SelectedCategory.CategoryID,
                    IsActive = true,
                    OrderBy = currentOrderBy,
                    SortDirection = currentOrderBy == "price" ? SortDirection.Asc : SortDirection.Desc,
                    MinPrice = minPrice > 0 ? minPrice : null,
                    MaxPrice = maxPrice < 10000000 ? maxPrice : null
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

        // Price Slider Event Handlers - Fixed Implementation
        private void PriceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Check if controls are initialized
            if (MinPriceSlider == null || MaxPriceSlider == null || PriceRangeDisplay == null)
                return;

            try
            {
                // Ensure min is not greater than max
                if (MinPriceSlider.Value > MaxPriceSlider.Value)
                {
                    if (sender == MinPriceSlider)
                        MaxPriceSlider.Value = MinPriceSlider.Value;
                    else
                        MinPriceSlider.Value = MaxPriceSlider.Value;
                }

                // Update display text
                PriceRangeDisplay.Text = $"{MinPriceSlider.Value:N0}đ - {MaxPriceSlider.Value:N0}đ";

                // Update price values
                minPrice = (decimal)MinPriceSlider.Value;
                maxPrice = (decimal)MaxPriceSlider.Value;

                // Use timer for delayed filtering - Fixed timer usage
                if (priceFilterTimer != null)
                {
                    priceFilterTimer.Stop();
                    priceFilterTimer.Start();
                }
                else
                {
                    // Fallback: filter immediately if timer is not available
                    FilterProductsByPrice();
                }
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                System.Diagnostics.Debug.WriteLine($"Error in PriceSlider_ValueChanged: {ex.Message}");
                // Fallback: filter immediately
                FilterProductsByPrice();
            }
        }

        private void PriceFilterTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (priceFilterTimer != null)
                {
                    priceFilterTimer.Stop();
                }
                FilterProductsByPrice();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PriceFilterTimer_Tick: {ex.Message}");
            }
        }

        private void FilterProductsByPrice()
        {
            currentPage = 1; // Reset to first page when filtering
            LoadProducts(); // Reload products with price filter
        }

        // Other Event Handlers
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

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                currentOrderBy = selectedItem.Tag.ToString();
                currentPage = 1;
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

        private void SearchProductTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchProductTextBox.Text == searchPlaceholder)
            {
                SearchProductTextBox.Text = "";
                SearchProductTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchProductTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchProductTextBox.Text))
            {
                SearchProductTextBox.Text = searchPlaceholder;
                SearchProductTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
            }
        }

        private void SearchProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchProductTextBox.Text != searchPlaceholder)
            {
                currentSearchTerm = SearchProductTextBox.Text;
                currentPage = 1;
                LoadProducts();
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

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = App.AppHost.Services.GetRequiredService<ProfileWindow>();
            profileWindow.Account = Account;
            profileWindow.Show();
            Close();
        }

        // Cleanup when window is closed
        protected override void OnClosed(EventArgs e)
        {
            if (priceFilterTimer != null)
            {
                priceFilterTimer.Stop();
                priceFilterTimer.Tick -= PriceFilterTimer_Tick;
                priceFilterTimer = null;
            }
            base.OnClosed(e);
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