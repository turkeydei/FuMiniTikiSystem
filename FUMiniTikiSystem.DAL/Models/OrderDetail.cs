using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUMiniTikiSystem.DAL.Models
{
    public class OrderDetail
    {
        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }


        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }
    }
}
