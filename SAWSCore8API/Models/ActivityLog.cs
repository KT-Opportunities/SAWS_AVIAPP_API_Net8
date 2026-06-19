using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [Table("ActivityLog")]
    public class ActivityLog
    {
        [Key]
        public int activityLogId { get; set; }

        //Activities details
        [MaxLength(150)]
        public string activityAction { get; set; }
        [MaxLength(50)]
        public string activityType { get; set; }
        [MaxLength(500)]
        public string activityDescription { get; set; }
        [MaxLength(256)]
        public string createdby_aspnetuserId { get; set; }
        [MaxLength(150)]
        public string createdby_aspnetusername { get; set; }
        [MaxLength(150)]
        public string path { get; set; }
        [MaxLength(50)]
        public string remoteipaddress { get; set; }

        //device info
        [MaxLength(150)]
        public string UserAgent { get; set; } = string.Empty;
        public bool? IsMobile { get; set; }
        [MaxLength(150)]
        public string DeviceType { get; set; } = string.Empty; // mobile, tablet, desktop, etc.
        [MaxLength(150)]
        public string Brand { get; set; } = string.Empty;
        [MaxLength(150)]
        public string Model { get; set; } = string.Empty;
        [MaxLength(150)]
        public string OsName { get; set; } = string.Empty;
        [MaxLength(150)]
        public string OsVersion { get; set; } = string.Empty;
        [MaxLength(150)]
        public string BrowserName { get; set; } = string.Empty;
        [MaxLength(150)]
        public string BrowserVersion { get; set; } = string.Empty;
        [MaxLength(150)]
        public string ClientHints { get; set; } = string.Empty;

        //timestamps

        public DateTime? created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }=DateTime.Now;
        public bool? isdeleted { get; set; } = false;
        public DateTime? deleted_at { get; set; }
    }
}
