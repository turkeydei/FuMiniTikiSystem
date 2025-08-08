using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class ProductRepo : GenericRepository<Product>
    {
        public ProductRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }

        public Product Get(int id)
        {
            return context.Products
                .Include(c => c.Category)
                .FirstOrDefault(c => c.ProductID == id);
        }

        public List<Product> GetAll()
        {
            return context.Products
                .Include(c => c.Category)
                .ToList();
        }

        public IQueryable<Product> GetAllProductsQueryable()
        {
            return context.Products
                .Include(c => c.OrderDetails)
                    .ThenInclude(od => od.Order)
                .Include(p => p.Reviews)
                .AsQueryable();
        }
    }
}
