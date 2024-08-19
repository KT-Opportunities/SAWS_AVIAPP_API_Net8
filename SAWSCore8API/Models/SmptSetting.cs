using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class SmptSetting
    {
        public string host { get; set; }
        public string userName { get; set; }
        public string Password { get; set; }
        public bool defaultCredentials { get; set; }
        public int Port { get; set; }
        public bool enableSsl { get; set; }
        public string from { get; set; }
        public string regUrl { get; set; }
        public string applicationUrl { get; set; }

    }
}
