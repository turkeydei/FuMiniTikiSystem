using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class CartRepo : GenericRepository<Cart>
    {
        public CartRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }

        public Cart Get(int id)
        {
            return context.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(c => c.CartID == id);
        }

        public Cart GetCartByCustomerID(int customerId)
        {
            var cart = context.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerID == customerId && c.IsActive);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerID = customerId,
                    IsActive = true
                };
                Create(cart);
            }

            return cart;
        }

        public List<Cart> GetAll()
        {
            return context.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(od => od.Product)
                .ToList();
        }
    }
}
