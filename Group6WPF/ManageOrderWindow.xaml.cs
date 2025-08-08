using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL.Models;
using System.Windows;
using System.Windows.Controls;

namespace Group6WPF
{
    /// <summary>
    /// Interaction logic for ManageOrderWindow.xaml
    /// </summary>
    public partial class ManageOrderWindow : Window
    {
        private OrderService orderService;
        private ProductService productService;
        private OrderDetailService orderDetailService;

        public ManageOrderWindow(OrderService orderService, ProductService productService, OrderDetailService orderDetailService)
        {
            InitializeComponent();
            this.orderService = orderService;
            this.productService = productService;
            this.orderDetailService = orderDetailService;
            LoadOrder();
            LoadProduct();
            LoadCustomer();
        }
        private void LoadOrder()
        {
            // Giả sử bạn đã có một phương thức để lấy danh sách đơn hàng từ service
            var orders = orderService.GetAllOrders();
            dgOrder.ItemsSource = orders;
            //dgOrderDetail.ItemsSource = orders.SelectMany(o => o.OrderDetails).ToList();
        }
        private void LoadProduct()
        {
            var products = productService.GetAllProducts();
            cbProductName.ItemsSource = products;
            cbProductName.DisplayMemberPath = "Name";
            cbProductName.SelectedValuePath = "ProductID";
        }
        private void LoadCustomer()
        {
            var customers = orderService.GetAllOrders().Select(o => o.Customer).Distinct().ToList();
            cbCustomerName.ItemsSource = customers;
            cbCustomerName.DisplayMemberPath = "Name";
            cbCustomerName.SelectedValuePath = "CustomerID";
        }

        private void dgOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDF();
            var selectedOrder = dgOrder.SelectedItem as FUMiniTikiSystem.DAL.Models.Order;
            if (selectedOrder != null)
            {
                txtId.Text = selectedOrder.OrderID.ToString();
                cbCustomerName.Text = selectedOrder.Customer?.Name ?? "Unknown";
                txtOrderAmount.Text = selectedOrder.OrderAmount.ToString("N2");
                dpOrderDate.Text = selectedOrder.OrderDate.ToString("dd/MM/yyyy");
                cbOrderStatus.Text = selectedOrder.OrderStatus;
                dgOrderDetail.ItemsSource = selectedOrder.OrderDetails.ToList();
            }
        }

        private void dgOrderDetail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOrderDetail = dgOrderDetail.SelectedItem as FUMiniTikiSystem.DAL.Models.OrderDetail;
            if (selectedOrderDetail != null)
            {
                cbProductName.Text = selectedOrderDetail.Product.Name;
                txtQuantity.Text = selectedOrderDetail.Quantity.ToString();
                txtUnitPrice.Text = selectedOrderDetail.UnitPrice.ToString("N2");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.ToLower().Trim();
            var orders = orderService.GetAllOrders();

            var filtered = orders.Where(o =>
                (!string.IsNullOrEmpty(o.Customer?.Name) && o.Customer.Name.ToLower().Contains(keyword)) ||
                o.OrderDate.ToString("dd/MM/yyyy").Contains(keyword)
            ).ToList();

            dgOrder.ItemsSource = filtered;

            // Đồng thời load OrderDetails của các đơn hàng tìm được
            //dgOrderDetail.ItemsSource = filtered.SelectMany(o => o.OrderDetails).ToList();
        }


        private void txtSearchD_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearchD.Text.ToLower().Trim();

            // Lấy Order hiện tại đang chọn
            var selectedOrder = dgOrder.SelectedItem as Order;
            if (selectedOrder != null)
            {
                var details = selectedOrder.OrderDetails
                    .Where(od => !string.IsNullOrEmpty(od.Product?.Name) && od.Product.Name.ToLower().Contains(keyword))
                    .ToList();

                dgOrderDetail.ItemsSource = details;
            }
            else
            {
                dgOrderDetail.ItemsSource = null;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Xử lý khi người dùng chọn item trong ComboBox
        }

        private void RefreshO()
        {
            dgOrder.SelectedItem = null;
            txtId.Clear();
            cbCustomerName.SelectedIndex = -1;
            txtOrderAmount.Clear();
            dpOrderDate.SelectedDate = null;
            cbOrderStatus.SelectedIndex = -1;
            RefreshD();
            dgOrderDetail.ItemsSource = null;
        }
        private void RefreshD()
        {
            LoadOrder();
            dgOrderDetail.SelectedItem = null;
            cbProductName.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
        }
        private void RefreshDF()
        {
            dgOrderDetail.SelectedItem = null;
            cbProductName.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
        }
        private bool ValidateOrderInput()
        {
            if (string.IsNullOrWhiteSpace(cbCustomerName.Text) ||
                string.IsNullOrWhiteSpace(txtOrderAmount.Text) ||
                dpOrderDate.SelectedDate == null ||
                cbOrderStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields.");
                return false;
            }
            return true;
        }
        private bool ValidateOrderDetailInput()
        {
            if (cbProductName.SelectedIndex == -1 ||
                string.IsNullOrWhiteSpace(txtQuantity.Text) ||
                string.IsNullOrWhiteSpace(txtUnitPrice.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return false;
            }
            if (!int.TryParse(txtQuantity.Text, out _) || !decimal.TryParse(txtUnitPrice.Text, out _))
            {
                MessageBox.Show("Invalid quantity or unit price.");
                return false;
            }
            return true;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshO();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOrderInput()) return;
            var order = new FUMiniTikiSystem.DAL.Models.Order
            {
                CustomerID = (int)cbCustomerName.SelectedValue,
                OrderAmount = decimal.Parse(txtOrderAmount.Text),
                OrderDate = dpOrderDate.SelectedDate.Value,
                OrderStatus = cbOrderStatus.Text
            };
            orderService.AddOrder(order);
            RefreshO();
            MessageBox.Show("Order added successfully.");
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOrderInput()) return;
            var selectedOrder = dgOrder.SelectedItem as FUMiniTikiSystem.DAL.Models.Order;
            if (selectedOrder != null)
            {
                selectedOrder.CustomerID = (int)cbCustomerName.SelectedValue;
                selectedOrder.OrderAmount = decimal.Parse(txtOrderAmount.Text);
                selectedOrder.OrderDate = dpOrderDate.SelectedDate.Value;
                selectedOrder.OrderStatus = cbOrderStatus.Text;
                orderService.UpdateOrder(selectedOrder);
                RefreshO();
                MessageBox.Show("Order updated successfully.");
            }
            else MessageBox.Show("Please select an order to edit.");

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = dgOrder.SelectedItem as FUMiniTikiSystem.DAL.Models.Order;

            if (selectedOrder != null)
            {
                if (selectedOrder.OrderStatus == "Completed")
                {
                    MessageBox.Show("Can't delete a completed order");
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this order?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    selectedOrder.OrderStatus = "Cancelled";
                    orderService.UpdateOrder(selectedOrder);
                    RefreshO();
                    MessageBox.Show("Order deleted successfully.");
                }
            }
            else MessageBox.Show("Please select an order to delete.");
        }


        private void btnRefreshD_Click(object sender, RoutedEventArgs e)
        {
            RefreshDF();
        }

        private void btnAddD_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOrderDetailInput()) return;
            var orderDetail = new OrderDetail
            {
                OrderID = int.Parse(txtId.Text),
                ProductID = (int)cbProductName.SelectedValue,
                Quantity = int.Parse(txtQuantity.Text),
                UnitPrice = decimal.Parse(txtUnitPrice.Text, System.Globalization.CultureInfo.InvariantCulture)
            };
            orderDetailService.AddOrderDetail(orderDetail);
            RefreshD();
            MessageBox.Show("Order detail added successfully.");
        }

        private void btnEditD_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOrderDetailInput()) return;
            var orderDetail = dgOrderDetail.SelectedItem as OrderDetail;
            if (orderDetail != null)
            {
                //orderDetail.ProductID = (int)cbProductName.SelectedValue;
                orderDetail.Quantity = int.Parse(txtQuantity.Text);
                orderDetail.UnitPrice = decimal.Parse(txtUnitPrice.Text);
                orderDetailService.UpdateOrderDetail(orderDetail);

                RefreshD();
                MessageBox.Show("Order detail updated successfully.");
            }
            else MessageBox.Show("Please select an order detail to edit.");
        }

        private void btnDeleteD_Click(object sender, RoutedEventArgs e)
        {

            var orderDetail = dgOrderDetail.SelectedItem as OrderDetail;

            if (orderDetail != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete this order detail?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Xoá trong DB
                    orderDetailService.DeleteOrderDetail(orderDetail.ProductID, orderDetail.OrderID);

                    // Xoá item khỏi DataGrid
                    var list = dgOrderDetail.ItemsSource as IList<OrderDetail>;
                    list?.Remove(orderDetail);
                    RefreshDF();

                    // Refresh lại hiển thị
                    dgOrderDetail.Items.Refresh();

                    MessageBox.Show("Order detail deleted successfully.");

                }
            }
            else MessageBox.Show("Please select an order detail to delete.");
        }



    }
}
