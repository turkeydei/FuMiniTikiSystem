using FUMiniTikiSystem.DAL.DBContext;

namespace FUMiniTikiSystem.BLL
{
    public class DatabaseMaintenanceService
    {
        private readonly FUMiniTikiSystemDbContext _context;

        public DatabaseMaintenanceService(FUMiniTikiSystemDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Reset identity seed cho tất cả các bảng
        /// </summary>
        public void ResetAllIdentitySeeds()
        {
            try
            {
                _context.ResetAllIdentitySeeds();
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error resetting identity seeds: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reset identity seed cho một bảng cụ thể
        /// </summary>
        /// <param name="tableName">Tên bảng</param>
        public void ResetIdentitySeed(string tableName)
        {
            try
            {
                _context.ResetIdentitySeed(tableName);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error resetting identity seed for table {tableName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Xóa tất cả dữ liệu và reset identity seed
        /// </summary>
        public void ClearAllDataAndResetSeeds()
        {
            try
            {
                // Xóa dữ liệu theo thứ tự để tránh lỗi foreign key
                _context.CartItems.RemoveRange(_context.CartItems);
                _context.Carts.RemoveRange(_context.Carts);
                _context.OrderDetails.RemoveRange(_context.OrderDetails);
                _context.Orders.RemoveRange(_context.Orders);
                _context.Reviews.RemoveRange(_context.Reviews);
                _context.Products.RemoveRange(_context.Products);
                _context.Categories.RemoveRange(_context.Categories);
                _context.Customers.RemoveRange(_context.Customers);
                _context.Payments.RemoveRange(_context.Payments);

                _context.SaveChanges();

                // Reset identity seeds
                ResetAllIdentitySeeds();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error clearing data and resetting seeds: {ex.Message}", ex);
            }
        }
    }
}