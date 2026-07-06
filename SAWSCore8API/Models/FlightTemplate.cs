using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [Table("FlightTemplate")]
    public class FlightTemplate
    {
        [Key]
        public int flightTemplateId { get; set; }

        [MaxLength(150)]
        public string templateName { get; set; }

        [MaxLength(50)]
        public string flightNumber { get; set; }

        [MaxLength(10)]
        public string departureICAO { get; set; }

        [MaxLength(10)]
        public string? enRouteICAO { get; set; }

        [MaxLength(10)]
        public string destinationICAO { get; set; }

        [MaxLength(4)]
        public string etd { get; set; }

        [MaxLength(4)]
        public string ete { get; set; }

        // User who owns this template
        [MaxLength(256)]
        public string createdby_aspnetuserId { get; set; }

        [MaxLength(150)]
        public string createdby_aspnetusername { get; set; }

        public DateTime? created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = DateTime.Now;
        public bool? isdeleted { get; set; } = false;
        public DateTime? deleted_at { get; set; }
    }
}
