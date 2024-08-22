using Microsoft.AspNetCore.Identity;
using SAWSCore8API.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Dtos
{
    [NotMapped]
    public class AdvertDto
    {
        public int advertId { get; set; }
        public string advert_caption { get; set; }
        public string uploaded_by { get; set; }
        public string advert_url { get; set; }
        public bool? ispublished { get; set; }
        public string file_url { get; set; }
        // public virtual List<DocAdvert> DocAdverts { get; set; } = new List<DocAdvert>();
    }

}

