using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class IDResetPassword
    {
        public string email { get; set; }
        public string token { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
    }
}
