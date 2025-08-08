using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class OrderDetailRepo : GenericRepository<OrderDetail>
    {
        public OrderDetailRepo(FUMiniTikiSystemDbContext context) : base(context) { }

        public OrderDetail Get(int productId, int orderId)
        {
            return context.OrderDetails
                .FirstOrDefault(od => od.ProductID == productId && od.OrderID == orderId);
        }

        public List<OrderDetail> GetAll()
        {
            return context.OrderDetails.Include(od => od.Product).Include(od => od.Order).ToList();
        }

        public void Delete(int productId, int orderId)
        {
            var detail = Get(productId, orderId);
            if (detail != null)
            {
                context.OrderDetails.Remove(detail);
                context.SaveChanges();
            }
        }
    }

}
