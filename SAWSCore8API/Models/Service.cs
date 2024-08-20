using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models
{
    [Table("Service")]
    public class Service
    {
        [Key]
        public int serviceId { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public int? packageId { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool? isdeleted { get; set; }
        public DateTime? deleted_at { get; set; }

        public virtual List<ServiceProduct> Products { get; set; } = new List<ServiceProduct>();

    }
}
