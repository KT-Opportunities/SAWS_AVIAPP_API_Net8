using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Dtos
{
    [NotMapped]
    public class AdvertDto
    {
        public int advertId { get; set; }
        public string advert_url { get; set; }
        public string file_url { get; set; }
    }

}

