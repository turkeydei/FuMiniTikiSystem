using FUMiniTikiSystem.DAL.Models;

namespace FUMiniTikiSystem.BLL
{
    public class CustomerService
    {
        private readonly FUMiniTikiSystem.DAL.CustomerRepo customerRepo;

        public CustomerService(FUMiniTikiSystem.DAL.CustomerRepo customerRepo)
        {
            this.customerRepo = customerRepo;
        }

        public FUMiniTikiSystem.DAL.Models.Customer GetCustomer(int id)
        {
            return customerRepo.Get(id);
        }

        public FUMiniTikiSystem.DAL.Models.Customer Login(string email, string password)
        {
            return customerRepo.Login(email, password);
        }

        public List<FUMiniTikiSystem.DAL.Models.Customer> GetAllCustomers()
        {
            return customerRepo.GetAll();
        }

        public void AddCustomer(FUMiniTikiSystem.DAL.Models.Customer customer)
        {
            customerRepo.Create(customer);
        }

        public void UpdateCustomer(FUMiniTikiSystem.DAL.Models.Customer customer)
        {
            customerRepo.Update(customer);
        }

        public void DeleteCustomer(int id)
        {
            var customer = customerRepo.Get(id);
            if (customer != null)
            {
                customer.IsActive = false; // Soft delete
                customerRepo.Update(customer);
            }
        }

        //public void DeleteCustomer(int id)
        //{
        //    var customer = customerRepo.Get(id);
        //    if (customer != null)
        //    {
        //        customerRepo.Remove(customer);
        //    }
        //}

        public Customer GetCustomerByEmail(string email)
        {
            return customerRepo.GetAll().FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
