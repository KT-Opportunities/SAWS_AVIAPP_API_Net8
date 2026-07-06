using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.DTOs.FlightTemplates
{
    public class CreateFlightTemplateRequest
    {
        [Required]
        [MaxLength(150)]
        public string templateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string flightNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string departureICAO { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? enRouteICAO { get; set; }

        [Required]
        [MaxLength(10)]
        public string destinationICAO { get; set; } = string.Empty;

        [Required]
        [MaxLength(4)]
        public string etd { get; set; } = string.Empty;

        [Required]
        [MaxLength(4)]
        public string ete { get; set; } = string.Empty;
    }
}
