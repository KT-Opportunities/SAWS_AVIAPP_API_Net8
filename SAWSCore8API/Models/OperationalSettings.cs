using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models
{
    public class OperationalSettings
    {
        [Key]
        public int OperationalSettingsId { get; set; }

        public string? PilotName { get; set; }
        public string? PilotLicense { get; set; }
        public string? DispatcherName { get; set; }
        public string? DispatcherLicense { get; set; }

        public string createdby_aspnetuserId { get; set; }
        public string createdby_aspnetusername { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public bool isdeleted { get; set; }
    }
}
