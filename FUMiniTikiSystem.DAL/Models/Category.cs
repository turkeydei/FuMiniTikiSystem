using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUMiniTikiSystem.DAL.Models
{
    public class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }

        public string Name { get; set; }

        public string Picture { get; set; }

        public string Description { get; set; }

        public bool? IsActive { get; set; } = true;


        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
