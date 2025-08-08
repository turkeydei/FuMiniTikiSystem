using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class CartItemRepo : GenericRepository<CartItem>
    {
        public CartItemRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }

        public CartItem Get(int productID, int cartID)
        {
            return context.CartItems
                .Include(ci => ci.Product).AsNoTracking()
                .FirstOrDefault(ci => ci.CartID == cartID && ci.ProductID == productID);
        }
    }
}
