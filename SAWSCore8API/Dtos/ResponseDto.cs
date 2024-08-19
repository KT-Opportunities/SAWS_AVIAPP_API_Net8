using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Dtos
{
    [NotMapped]
    public class ResponseDto
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
