using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUMiniTikiSystem.DAL.Models
{
    public class Payment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentID { get; set; }

        [Required, ForeignKey("Order")]
        public int OrderID { get; set; }

        [Required]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public string PaymentStatus { get; set; }

        [Required]
        public string PaymentMethod { get; set; }


        // Navigation property
        public virtual Order Order { get; set; }
    }
}
