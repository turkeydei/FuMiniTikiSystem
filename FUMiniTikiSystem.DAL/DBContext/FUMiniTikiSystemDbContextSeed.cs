using FUMiniTikiSystem.DAL.Constants;
using FUMiniTikiSystem.DAL.Helper;
using FUMiniTikiSystem.DAL.Models;

namespace FUMiniTikiSystem.DAL.DBContext
{
    public class FUMiniTikiSystemDbContextSeed
    {
        private readonly FUMiniTikiSystemDbContext _dbContext;
        private readonly IFileReader _fileReader;

        public FUMiniTikiSystemDbContextSeed(FUMiniTikiSystemDbContext dbContext, IFileReader fileReader)
        {
            _dbContext = dbContext;
            _fileReader = fileReader;
        }

        public void Seed()
        {
            var rootPath = AppCts.AbsoluteProjectPath;

            new JsonDataSeeder<Category, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.CategoryPath)
               .Seed();

            new JsonDataSeeder<Product, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.ProductPath)
               .Seed();

            new JsonDataSeeder<Customer, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.CustomerPath)
               .Seed();

            new JsonDataSeeder<Cart, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.CartPath)
               .Seed();

            new JsonDataSeeder<CartItem, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.CartItemPath)
               .Seed();

            new JsonDataSeeder<Review, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.ReviewPath)
               .Seed();

            new JsonDataSeeder<Order, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.OrderPath)
               .Seed();

            new JsonDataSeeder<OrderDetail, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.OrderDetailPath)
               .Seed();

            new JsonDataSeeder<Payment, FUMiniTikiSystemDbContext>(_fileReader, _dbContext)
               .AddRelativeFilePath(rootPath, AppCts.SeederRelativePath.PaymentPath)
               .Seed();
        }

    }
}
