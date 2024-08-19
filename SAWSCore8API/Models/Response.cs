using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public IdentityResult Detail { get; set; }
        public object DetailDescription { get; set; }
    }
}
