using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class OrderRepo : GenericRepository<Order>
    {
        public OrderRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }

        public Order Get(int id)
        {
            return context.Orders
                .Include(c => c.Customer)
                .Include(c => c.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(c => c.OrderID == id);
        }

        public List<Order> GetAll()
        {
            return context.Orders
                .Include(c => c.Customer)
                .Include(c => c.OrderDetails)
                    .ThenInclude(od => od.Product)
                .ToList();
        }

        //public List<Order> GetCustomerOrders(int customerID)
        //{
        //    return context.Orders
        //        .Where(o => o.CustomerID == customerID)
        //        .Include(c => c.Customer)
        //        .Include(c => c.OrderDetails)
        //            .ThenInclude(od => od.Product)
        //        .OrderByDescending(o => o.OrderDate)
        //        .ToList();
        //}
    }
}
