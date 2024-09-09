using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models
{
    [Table("Package")]
    public class Package
    {
        [Key]
        public int packageId { get; set; }
        [Required]
        public string name { get; set; }

        [Range(0.01, 9999.99)]
        public Decimal price { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool? isdeleted { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
