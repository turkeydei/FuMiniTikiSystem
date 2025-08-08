using FUMiniTikiSystem.BLL;
using FUMiniTikiSystem.DAL;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Group6WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<FUMiniTikiSystemDbContext>(options =>
                        options.UseSqlServer(FUMiniTikiSystemDbContext.GetConnectionString("DefaultConnection")));
                    services.AddScoped<FUMiniTikiSystemDbContextSeed>();

                    services.AddScoped<IFileReader, FileReader>();
                    services.AddScoped<CustomerRepo>();
                    services.AddScoped<OrderRepo>();
                    services.AddScoped<ProductRepo>();
                    services.AddScoped<CategoryRepo>();
                    services.AddScoped<OrderDetailRepo>();
                    services.AddScoped<CartRepo>();
                    services.AddScoped<CartItemRepo>();
                    services.AddScoped<PaymentRepo>();

                    services.AddScoped<CustomerService>();
                    services.AddScoped<ProductService>();
                    services.AddScoped<OrderService>();
                    services.AddScoped<CategoryService>();
                    services.AddScoped<OrderDetailService>();
                    services.AddScoped<CartService>();
                    services.AddScoped<CartItemService>();
                    services.AddScoped<PaymentService>();
                    services.AddScoped<DatabaseMaintenanceService>();
                    services.AddScoped<VNPAYService>();


                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<AdminWindow>();
                    services.AddTransient<CartWindow>();
                    services.AddTransient<ProductDetailWindow>();
                    services.AddTransient<ProfileWindow>();
                    services.AddTransient<SignUpWindow>();
                    services.AddTransient<ProductMainWindow>();
                    services.AddTransient<ManageOrderWindow>();
                    services.AddTransient<ManageCategory>();
                    services.AddTransient<CustomerDetailWindow>();
                    services.AddTransient<CustomerManagementWindow>();
                    services.AddTransient<DatabaseMaintenanceWindow>();
                    services.AddTransient<CheckOutWindow>();
                    services.AddTransient<VNPayWindow>();
                    services.AddTransient<ViewOrderHistory>();
                    services.AddTransient<ProductSearchWindow>();
                    services.AddTransient<CategoryForm>();
                    services.AddTransient<CategoryMainWindow>();
                })
                .Build();

            using (var scope = AppHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var fuMiniTikiSystemDbContext = services.GetRequiredService<FUMiniTikiSystemDbContext>();
                var fuMiniTikiSystemDbContextSeed =
                    scope.ServiceProvider.GetRequiredService<FUMiniTikiSystemDbContextSeed>();
                var logger = services.GetRequiredService<ILogger<App>>();

                try
                {
                    fuMiniTikiSystemDbContext.Database.Migrate();
                    fuMiniTikiSystemDbContextSeed.Seed();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                    throw;
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Start DI host
            AppHost.Start();

            var loginWindow = AppHost.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }
}
