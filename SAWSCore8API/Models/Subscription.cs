using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models
{
    [Table("Subscription")]
    public class Subscription
    {
        [Key]
        public int subscriptionId { get; set; }
        public int userprofileid { get; set; }
        [Required]
        public string package_name { get; set; }
        public int package_id { get; set; }

        [Range(0.01, 9999.99)]
        public Decimal package_price { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int subscription_duration { get; set; }
        public string subscription_token { get; set; }
        public string subscription_status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool? isdeleted { get; set; }
        public DateTime? deleted_at { get; set; }

    }
}
