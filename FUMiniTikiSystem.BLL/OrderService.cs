

namespace FUMiniTikiSystem.BLL
{
    public class OrderService
    {
        private readonly FUMiniTikiSystem.DAL.OrderRepo orderRepo;

        public OrderService(FUMiniTikiSystem.DAL.OrderRepo orderRepo)
        {
            this.orderRepo = orderRepo;
        }

        public FUMiniTikiSystem.DAL.Models.Order GetOrder(int id)
        {
            return orderRepo.Get(id);
        }

        public List<FUMiniTikiSystem.DAL.Models.Order> GetAllOrders()
        {
            return orderRepo.GetAll();
        }

        public List<FUMiniTikiSystem.DAL.Models.Order> GetCustomerOrders(int customerID)
        {
            return orderRepo.GetAll()
                .Where(o => o.CustomerID == customerID)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public void AddOrder(FUMiniTikiSystem.DAL.Models.Order order)
        {
            orderRepo.Create(order);
        }

        public void UpdateOrder(FUMiniTikiSystem.DAL.Models.Order order)
        {
            orderRepo.Update(order);
        }

        public void DeleteOrder(int id)
        {
            var order = orderRepo.Get(id);
            if (order != null)
            {
                orderRepo.Remove(order);
            }
        }

        public List<decimal> GetRevenueLastWeek()
        {
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-6); // Last 7 days including today

            var orders = orderRepo.GetAll()
                .Where(o => o.OrderDate.Date >= startDate && o.OrderDate.Date <= endDate && o.OrderStatus == "Completed")
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(o => o.OrderAmount)
                })
                .ToDictionary(x => x.Date, x => x.Total);

            var revenueList = new List<decimal>();

            // Generate revenue for each day in chronological order (oldest to newest)
            for (int i = 6; i >= 0; i--) // Changed from 0 to 6 to 6 to 0
            {
                var date = endDate.AddDays(-i);
                var revenue = orders.ContainsKey(date) ? orders[date] : 0;
                revenueList.Add(revenue);
            }

            return revenueList;
        }
    }
}
