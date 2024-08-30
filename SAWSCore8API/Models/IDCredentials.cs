using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class IDCredentials
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
