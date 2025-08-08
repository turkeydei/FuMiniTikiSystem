namespace FUMiniTikiSystem.BLL
{
    public class CartService
    {
        private readonly FUMiniTikiSystem.DAL.CartRepo cartRepo;

        public CartService(FUMiniTikiSystem.DAL.CartRepo cartRepo)
        {
            this.cartRepo = cartRepo;
        }

        public FUMiniTikiSystem.DAL.Models.Cart GetCartByCustomerID(int customerId)
        {
            return cartRepo.GetCartByCustomerID(customerId);
        }

        public void Update(FUMiniTikiSystem.DAL.Models.Cart cart)
        {
            cartRepo.Update(cart);
        }
    }
}
