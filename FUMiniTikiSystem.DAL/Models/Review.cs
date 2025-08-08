using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUMiniTikiSystem.DAL.Models
{
    public class Review
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewID { get; set; }

        [Required, ForeignKey("Product")]
        public int ProductID { get; set; }

        [Required, ForeignKey("Customer")]
        public int CustomerID { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } // from 1 to 5

        [Required]
        public string Comment { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
