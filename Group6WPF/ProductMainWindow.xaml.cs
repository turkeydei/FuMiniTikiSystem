using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Group6WPF
{
    public partial class ProductMainWindow : Window
    {
        private readonly OrderService orderService;
        private readonly CartService cartService;
        private readonly CartItemService cartItemService;

        private int quantity = 1;
        public Product SelectedProduct { get; set; }
        public Customer Account { get; set; }

        public string searchPlaceholder = "Search product by name...";
        public ProductMainWindow(OrderService orderService, CartService cartService, CartItemService cartItemService)
        {
            InitializeComponent();
            Loaded += ProductMainWindow_Loaded;

            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            this.cartItemService = cartItemService ?? throw new ArgumentNullException(nameof(cartItemService));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Account = Account;
            mainWindow.Show();
            Close();
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

        private void ProductMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct == null) return;

            ProductNameTextBlock.Text = SelectedProduct.Name;
            ProductDescriptionTextBlock.Text = SelectedProduct.Description;
            ProductPriceTextBlock.Text = $"{SelectedProduct.Price:N0}đ";
            QuantityTextBox.Text = quantity.ToString();
            UpdateTotalPrice();

            try
            {
                ProductImage.Source = new BitmapImage(new Uri(SelectedProduct.Picture, UriKind.Absolute));
            }
            catch
            {
                ProductImage.Source = new BitmapImage(new Uri("Assets/placeholder.png", UriKind.Relative));
            }

            // Quantity control events
            IncreaseButton.Click += (s, args) =>
            {
                quantity++;
                QuantityTextBox.Text = quantity.ToString();
                UpdateTotalPrice();
            };

            DecreaseButton.Click += (s, args) =>
            {
                if (quantity > 1)
                {
                    quantity--;
                    QuantityTextBox.Text = quantity.ToString();
                    UpdateTotalPrice();
                }
            };

            QuantityTextBox.TextChanged += (s, args) =>
            {
                if (int.TryParse(QuantityTextBox.Text, out int qty) && qty > 0)
                {
                    quantity = qty;
                    UpdateTotalPrice();
                }
            };

            // Action button events
            BuyNowButton.Click += BuyNowButton_Click;
            AddToCartButton.Click += AddToCartButton_Click;
            UpdateCartBadge();
        }

        private void UpdateTotalPrice()
        {
            decimal total = (SelectedProduct?.Price ?? 0) * quantity;
            TotalPriceTextBlock.Text = $"{total:N0}đ";
        }

        //private void BackButton_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}

        private void BuyNowButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var order = new Order()
            //    {
            //        CustomerID = Account.CustomerID,
            //        OrderAmount = quantity * SelectedProduct.Price,
            //        OrderStatus = "Pending",
            //        OrderDetails = new List<OrderDetail>
            //    {
            //        new OrderDetail
            //        {
            //            ProductID = SelectedProduct.ProductID,
            //            Quantity = quantity,
            //            UnitPrice = SelectedProduct.Price
            //        }
            //    }
            //    };

            //    orderService.AddOrder(order);
            //    MessageBox.Show("Place order successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            //    MainWindow mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
            //    mainWindow.Account = Account;
            //    mainWindow.Show();
            //    Close();
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("Failed to place order. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            try
            {
                if (SelectedProduct == null)
                {
                    MessageBox.Show("No product selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (quantity <= 0)
                {
                    MessageBox.Show("Quantity must be greater than zero.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var cart = cartService.GetCartByCustomerID(Account.CustomerID);

                if (cart == null)
                {
                    MessageBox.Show("No active cart found. Please create a cart first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingCartItem = cartItemService.Get(SelectedProduct.ProductID, cart.CartID);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += quantity;
                    cartItemService.Update(existingCartItem);
                    UpdateCartBadge();
                    MessageBox.Show("Added to cart successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var cartItem = new CartItem()
                {
                    ProductID = SelectedProduct.ProductID,
                    CartID = cart.CartID,
                    Quantity = quantity
                };
                cartItemService.Add(cartItem);
                UpdateCartBadge();
                MessageBox.Show("Added to cart successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to add to cart. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedProduct == null)
                {
                    MessageBox.Show("No product selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (quantity <= 0)
                {
                    MessageBox.Show("Quantity must be greater than zero.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var cart = cartService.GetCartByCustomerID(Account.CustomerID);

                if (cart == null)
                {
                    MessageBox.Show("No active cart found. Please create a cart first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingCartItem = cartItemService.Get(SelectedProduct.ProductID, cart.CartID);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += quantity;
                    cartItemService.Update(existingCartItem);
                    UpdateCartBadge();
                    MessageBox.Show("Added to cart successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var cartItem = new CartItem()
                {
                    ProductID = SelectedProduct.ProductID,
                    CartID = cart.CartID,
                    Quantity = quantity
                };
                cartItemService.Add(cartItem);
                UpdateCartBadge();
                MessageBox.Show("Added to cart successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to add to cart. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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