using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class CustomerRepo : GenericRepository<Customer>
    {
        public CustomerRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }

        public Customer Login(string email, string password)
        {
            return context.Customers.FirstOrDefault(c => c.Email == email && c.Password == password);
        }

        public Customer Get(int id)
        {
            return context.Customers
                .Include(c => c.Orders)
                .Include(c => c.Carts)
                .FirstOrDefault(c => c.CustomerID == id);
        }

        public List<Customer> GetAll()
        {
            return context.Customers
                .Include(c => c.Orders)
                .Include(c => c.Carts)
                .ToList();
        }
    }
}
