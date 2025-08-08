using System.ComponentModel.DataAnnotations.Schema;

namespace FUMiniTikiSystem.DAL.Models
{
    public class CartItem
    {
        [ForeignKey("Cart")]
        public int CartID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        public int? Quantity { get; set; }


        // Navigation properties
        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
    }
}
