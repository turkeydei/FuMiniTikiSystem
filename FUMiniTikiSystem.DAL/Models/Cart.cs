using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUMiniTikiSystem.DAL.Models
{
    public class Cart
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartID { get; set; }

        [Required, ForeignKey("Customer")]
        public int CustomerID { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;


        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
