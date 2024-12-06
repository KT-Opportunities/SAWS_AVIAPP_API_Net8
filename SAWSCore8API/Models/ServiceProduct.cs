using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models
{
    [Table("ServiceProduct")]
    public class ServiceProduct
    {
        [Key]
        public int serviceProductId { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public int serviceId { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool? isdeleted { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
