

namespace FUMiniTikiSystem.BLL
{
    public class PaymentService
    {
        private readonly FUMiniTikiSystem.DAL.PaymentRepo paymentRepo;

        public PaymentService(FUMiniTikiSystem.DAL.PaymentRepo paymentRepo)
        {
            this.paymentRepo = paymentRepo;
        }

        public FUMiniTikiSystem.DAL.Models.Payment GetPayment(int id)
        {
            return paymentRepo.GetById(id);
        }

        public List<FUMiniTikiSystem.DAL.Models.Payment> GetAllPayments()
        {
            return paymentRepo.GetAll();
        }

        public void AddPayment(FUMiniTikiSystem.DAL.Models.Payment payment)
        {
            paymentRepo.Create(payment);
        }

        public void UpdatePayment(FUMiniTikiSystem.DAL.Models.Payment payment)
        {
            paymentRepo.Update(payment);
        }

        public void DeletePayment(int id)
        {
            var payment = paymentRepo.GetById(id);
            if (payment != null)
            {
                paymentRepo.Remove(payment);
            }
        }
    }
}
