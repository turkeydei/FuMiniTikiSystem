using FUMiniTikiSystem.DAL;
using FUMiniTikiSystem.DAL.Models;

namespace FUMiniTikiSystem.BLL
{
    public class OrderDetailService
    {
        private readonly OrderDetailRepo _orderDetailRepo;

        public OrderDetailService(OrderDetailRepo orderDetailRepo)
        {
            _orderDetailRepo = orderDetailRepo ?? throw new ArgumentNullException(nameof(orderDetailRepo));
        }

        public List<OrderDetail> GetAllOrderDetails()
        {
            return _orderDetailRepo.GetAll();
        }

        public OrderDetail GetOrderDetail(int productId, int orderId)
        {
            return _orderDetailRepo.Get(productId, orderId);
        }

        public void AddOrderDetail(OrderDetail orderDetail)
        {
            if (orderDetail == null) throw new ArgumentNullException(nameof(orderDetail));
            _orderDetailRepo.Create(orderDetail);
        }

        public void UpdateOrderDetail(OrderDetail orderDetail)
        {
            if (orderDetail == null) throw new ArgumentNullException(nameof(orderDetail));
            _orderDetailRepo.UpdateV2(orderDetail);
        }

        public void DeleteOrderDetail(int productId, int orderId)
        {
            _orderDetailRepo.Delete(productId, orderId);
        }

        public List<OrderDetail> GetOrderDetailsByOrderID(int orderId)
        {
            return _orderDetailRepo.GetAll()
                .Where(od => od.OrderID == orderId)
                .ToList();
        }
    }
}
