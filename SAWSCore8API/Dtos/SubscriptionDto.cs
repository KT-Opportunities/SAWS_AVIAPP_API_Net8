using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Dtos
{
    [NotMapped]
    public class SubscriptionDto
    {
        public int subscriptionId { get; set; }
        public string package_name { get; set; }
        public bool isactive { get; set; }

    }
}
