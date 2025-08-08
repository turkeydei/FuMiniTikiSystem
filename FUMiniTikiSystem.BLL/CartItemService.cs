using FUMiniTikiSystem.DAL.Models;

namespace FUMiniTikiSystem.BLL
{
    public class CartItemService
    {
        private readonly FUMiniTikiSystem.DAL.CartItemRepo cartRepo;

        public CartItemService(FUMiniTikiSystem.DAL.CartItemRepo cartRepo)
        {
            this.cartRepo = cartRepo;
        }

        public CartItem Get(int productID, int cartID) => cartRepo.Get(productID, cartID);

        public void Delete(CartItem item)
        {
            cartRepo.RemoveV2(item);
        }

        public void Update(CartItem item)
        {
            cartRepo.UpdateV2(item);
        }

        public void Add(CartItem item)
        {
            cartRepo.Create(item);
        }
    }
}
