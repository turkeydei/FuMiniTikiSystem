using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;

namespace FUMiniTikiSystem.DAL
{
    public class PaymentRepo : GenericRepository<Payment>
    {
        public PaymentRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }
    }
}
