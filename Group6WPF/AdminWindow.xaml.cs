using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Group6WPF
{
    public partial class AdminWindow : Window
    {
        private readonly ProductService productService;
        private readonly CategoryService categoryService;
        private readonly OrderService orderService;
        private readonly CustomerService customerService;
        private DispatcherTimer timer;

        public AdminWindow(ProductService productService, CategoryService categoryService, OrderService orderService, CustomerService customerService)
        {
            InitializeComponent();
            this.productService = productService;
            this.categoryService = categoryService;
            this.orderService = orderService;
            this.customerService = customerService;

            InitializeTimer();
            LoadDashboard();
            SetupEventHandlers();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtCurrentTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");
        }

        private void SetupEventHandlers()
        {
            btnAddProduct.Click += BtnAddProduct_Click;
            btnEditProduct.Click += BtnEditProduct_Click;
            btnDeleteProduct.Click += BtnDeleteProduct_Click;
            //btnManageCategory.Click += BtnManageCategory_Click;
            //btnManageCustomer.Click += BtnManageCustomer_Click;
            btnManageProduct.Click += BtnManageProduct_Click;
            txtSearchProduct.KeyDown += TxtSearchProduct_KeyDown;
            btnReload.Click += (s, e) => ReloadUI();
        }

        private void LoadDashboard()
        {
            ShowDashboard();
            LoadDashboardStatistics();
            LoadCharts();
        }

        private void ShowDashboard()
        {
            DashboardContent.Visibility = Visibility.Visible;
            ProductManagementContent.Visibility = Visibility.Collapsed;

            // Update button states
            btnDashboard.Background = System.Windows.Media.Brushes.MediumSlateBlue;
            btnManageProduct.Background = System.Windows.Media.Brushes.SlateBlue;
        }

        private void ShowProductManagement()
        {
            DashboardContent.Visibility = Visibility.Collapsed;
            ProductManagementContent.Visibility = Visibility.Visible;
            LoadProducts();

            // Update button states
            btnDashboard.Background = System.Windows.Media.Brushes.SlateBlue;
            btnManageProduct.Background = System.Windows.Media.Brushes.MediumSlateBlue;
        }

        private void LoadDashboardStatistics()
        {
            try
            {
                // Load total products
                var products = productService.GetAllProducts();
                txtTotalProducts.Text = products.Count.ToString();

                var orders = orderService.GetAllOrders();
                txtTotalOrders.Text = orders.Count.ToString();

                var customers = customerService.GetAllCustomers();
                txtTotalCustomers.Text = customers.Count.ToString();

                decimal totalRevenue = orders.Where(o => o.OrderStatus == "Completed").Sum(o => o.OrderAmount);
                txtTotalRevenue.Text = $"{totalRevenue:N0}đ";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCharts()
        {
            try
            {
                LoadSalesChart();
                LoadCategoryChart();
                LoadOrderStatusChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading charts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSalesChart()
        {
            SalesChart.Children.Clear();

            try
            {
                // Get actual sales data for last 7 days
                var salesData = orderService.GetRevenueLastWeek();

                // Generate day labels (last 7 days)
                var dayLabels = new List<string>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.Date.AddDays(-i);
                    dayLabels.Add(date.ToString("ddd")); // Mon, Tue, Wed, etc.
                }

                // Handle empty data
                if (salesData == null || !salesData.Any() || salesData.All(x => x == 0))
                {
                    // Show "No Data" message
                    var noDataText = new TextBlock
                    {
                        Text = "No sales data available",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Canvas.SetLeft(noDataText, 100);
                    Canvas.SetTop(noDataText, 75);
                    SalesChart.Children.Add(noDataText);
                    return;
                }

                decimal maxSales = salesData.Max();
                if (maxSales == 0) maxSales = 1; // Prevent division by zero

                double chartHeight = 150;
                double chartWidth = SalesChart.ActualWidth > 0 ? SalesChart.ActualWidth - 40 : 300;
                double barWidth = chartWidth / salesData.Count;

                for (int i = 0; i < salesData.Count; i++)
                {
                    double barHeight = maxSales > 0 ? ((double)salesData[i] / (double)maxSales) * chartHeight : 0;

                    // Ensure minimum visible height for non-zero values
                    if (salesData[i] > 0 && barHeight < 5)
                        barHeight = 5;

                    // Create bar
                    var bar = new Rectangle
                    {
                        Width = barWidth - 10,
                        Height = barHeight,
                        Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                        Stroke = new SolidColorBrush(Color.FromRgb(21, 101, 192)),
                        StrokeThickness = 1
                    };

                    Canvas.SetLeft(bar, i * barWidth + 5);
                    Canvas.SetTop(bar, chartHeight - barHeight + 20);
                    SalesChart.Children.Add(bar);

                    // Create day label
                    var dayLabel = new TextBlock
                    {
                        Text = dayLabels[i],
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    Canvas.SetLeft(dayLabel, i * barWidth + barWidth / 2 - 10);
                    Canvas.SetTop(dayLabel, chartHeight + 25);
                    SalesChart.Children.Add(dayLabel);

                    // Create value label
                    var valueLabel = new TextBlock
                    {
                        Text = FormatCurrency(salesData[i]),
                        FontSize = 9,
                        Foreground = salesData[i] > 0 ? Brushes.White : Brushes.Gray,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    Canvas.SetLeft(valueLabel, i * barWidth + 5);
                    Canvas.SetTop(valueLabel, chartHeight - barHeight + 25);
                    SalesChart.Children.Add(valueLabel);

                    // Add date tooltip (optional)
                    var dateLabel = new TextBlock
                    {
                        Text = DateTime.Now.Date.AddDays(-(6 - i)).ToString("MM/dd"),
                        FontSize = 8,
                        Foreground = Brushes.LightGray,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    Canvas.SetLeft(dateLabel, i * barWidth + barWidth / 2 - 10);
                    Canvas.SetTop(dateLabel, chartHeight + 40);
                    SalesChart.Children.Add(dateLabel);
                }
            }
            catch (Exception ex)
            {
                // Show error message
                var errorText = new TextBlock
                {
                    Text = "Error loading sales data",
                    FontSize = 12,
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Canvas.SetLeft(errorText, 100);
                Canvas.SetTop(errorText, 75);
                SalesChart.Children.Add(errorText);
            }
        }

        // Helper method to format currency
        private string FormatCurrency(decimal amount)
        {
            if (amount >= 1000000)
                return $"{amount / 1000000:F1}M";
            else if (amount >= 1000)
                return $"{amount / 1000:F0}K";
            else
                return amount.ToString("F0");
        }

        private void LoadCategoryChart()
        {
            CategoryChart.Children.Clear();

            try
            {
                var categories = categoryService.GetAllCategories();
                var products = productService.GetAllProducts();

                var categoryData = categories.Select(c => new
                {
                    Name = c.Name,
                    Count = products.Count(p => p.CategoryID == c.CategoryID),
                    Color = GetRandomColor()
                }).OrderByDescending(x => x.Count).Take(5).ToList();

                int totalProducts = categoryData.Sum(x => x.Count);

                for (int i = 0; i < categoryData.Count; i++)
                {
                    var item = categoryData[i];
                    double percentage = totalProducts > 0 ? (double)item.Count / totalProducts * 100 : 0;

                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    // Color indicator
                    var colorRect = new Rectangle
                    {
                        Width = 20,
                        Height = 15,
                        Fill = new SolidColorBrush(item.Color),
                        Margin = new Thickness(0, 0, 10, 0)
                    };

                    // Category info
                    var infoText = new TextBlock
                    {
                        Text = $"{item.Name}: {item.Count} ({percentage:F1}%)",
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 150
                    };

                    // Progress bar
                    var progressBar = new ProgressBar
                    {
                        Width = 100,
                        Height = 15,
                        Value = percentage,
                        Maximum = 100,
                        Foreground = new SolidColorBrush(item.Color),
                        Margin = new Thickness(10, 0, 0, 0)
                    };

                    stackPanel.Children.Add(colorRect);
                    stackPanel.Children.Add(infoText);
                    stackPanel.Children.Add(progressBar);

                    CategoryChart.Children.Add(stackPanel);
                }
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = "Unable to load category data",
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CategoryChart.Children.Add(errorText);
            }
        }

        private void LoadOrderStatusChart()
        {
            try
            {
                var orders = orderService.GetAllOrders();
                var statusCounts = new Dictionary<string, int>
                {
                    ["Pending"] = orders.Count(o => o.OrderStatus == "Pending"),
                    ["Processing"] = orders.Count(o => o.OrderStatus == "Processing"),
                    ["Completed"] = orders.Count(o => o.OrderStatus == "Completed"),
                    ["Cancelled"] = orders.Count(o => o.OrderStatus == "Cancelled")
                };

                int totalOrders = statusCounts.Values.Sum();

                // Update progress bars and counts
                progressPending.Value = totalOrders > 0 ? (double)statusCounts["Pending"] / totalOrders * 100 : 0;
                txtPendingCount.Text = statusCounts["Pending"].ToString();

                progressProcessing.Value = totalOrders > 0 ? (double)statusCounts["Processing"] / totalOrders * 100 : 0;
                txtProcessingCount.Text = statusCounts["Processing"].ToString();

                progressCompleted.Value = totalOrders > 0 ? (double)statusCounts["Completed"] / totalOrders * 100 : 0;
                txtCompletedCount.Text = statusCounts["Completed"].ToString();

                progressCancelled.Value = totalOrders > 0 ? (double)statusCounts["Cancelled"] / totalOrders * 100 : 0;
                txtCancelledCount.Text = statusCounts["Cancelled"].ToString();
            }
            catch (Exception ex)
            {
                // Set default values on error
                progressPending.Value = 0;
                progressProcessing.Value = 0;
                progressCompleted.Value = 0;
                progressCancelled.Value = 0;

                txtPendingCount.Text = "0";
                txtProcessingCount.Text = "0";
                txtCompletedCount.Text = "0";
                txtCancelledCount.Text = "0";
            }
        }

        private Color GetRandomColor()
        {
            var colors = new Color[]
            {
                Color.FromRgb(76, 175, 80),   // Green
                Color.FromRgb(33, 150, 243),  // Blue
                Color.FromRgb(255, 152, 0),   // Orange
                Color.FromRgb(156, 39, 176),  // Purple
                Color.FromRgb(244, 67, 54),   // Red
                Color.FromRgb(96, 125, 139),  // Blue Grey
                Color.FromRgb(121, 85, 72),   // Brown
                Color.FromRgb(158, 158, 158)  // Grey
            };

            var random = new Random();
            return colors[random.Next(colors.Length)];
        }

        // Dashboard Navigation Event Handlers
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void BtnManageProduct_Click(object sender, RoutedEventArgs e)
        {
            ShowProductManagement();
        }

        private void BtnQuickAddProduct_Click(object sender, RoutedEventArgs e)
        {
            BtnAddProduct_Click(sender, e);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = App.AppHost.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                this.Close();
            }
        }

        // Existing methods from your original code
        private void ReloadUI()
        {
            if (ProductManagementContent.Visibility == Visibility.Visible)
            {
                LoadProducts();
                txtSearchProduct.Text = string.Empty;
                dgProducts.SelectedItem = null;
            }
            else
            {
                LoadDashboard();
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var productDetailWindow = App.AppHost.Services.GetRequiredService<ProductDetailWindow>();
            productDetailWindow.ShowDialog();
            ReloadUI();
            LoadDashboardStatistics();
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                var productDetailWindow = App.AppHost.Services.GetRequiredService<ProductDetailWindow>();
                productDetailWindow._product = selectedProduct;
                productDetailWindow.ShowDialog();
                ReloadUI();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TxtSearchProduct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string keyword = txtSearchProduct.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(keyword))
                {
                    LoadProducts();
                }
                else
                {
                    var allProducts = productService.GetAllProducts();
                    var filtered = allProducts.Where(p =>
                        p.Name.ToLower().Contains(keyword) ||
                        (p.Category != null && p.Category.Name.ToLower().Contains(keyword))
                    ).ToList();
                    dgProducts.ItemsSource = filtered;
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm '{selectedProduct.Name}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    productService.DeleteProduct(selectedProduct.ProductID);
                    MessageBox.Show("Đã xóa sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProducts();
                    LoadDashboardStatistics(); // Refresh dashboard stats
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadProducts()
        {
            List<Product> products = productService.GetAllProducts();
            dgProducts.ItemsSource = products;
        }

        private void btnManageOrder_Click(object sender, RoutedEventArgs e)
        {
            ManageOrderWindow manageOrderWindow = App.AppHost.Services.GetRequiredService<ManageOrderWindow>();
            manageOrderWindow.ShowDialog();
        }

        private void BtnManageCategory_Click(object sender, RoutedEventArgs e)
        {
            ManageCategory manageCategoryWindow = App.AppHost.Services.GetRequiredService<ManageCategory>();
            manageCategoryWindow.ShowDialog();
        }

        private void BtnManageCustomer_Click(object sender, RoutedEventArgs e)
        {
            CustomerManagementWindow customerManagementWindow = App.AppHost.Services.GetRequiredService<CustomerManagementWindow>();
            customerManagementWindow.ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }
    }
}