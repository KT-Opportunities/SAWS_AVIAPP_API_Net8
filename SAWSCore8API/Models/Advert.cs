using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models
{
    /// <summary>
    /// Represents an advert in the system.
    /// </summary>
    [Table("Advert")]
    public class Advert
    {
        /// <summary>
        /// Gets or sets the ID of the advert.
        /// </summary>
        [Key]
        public int advertId { get; set; }
        /// <summary>
        /// Gets or sets the advert caption.
        /// </summary>
        [Required]
        public string advert_caption { get; set; }
        /// <summary>
        /// Gets or sets the email address of the person uploading.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string uploaded_by { get; set; }
        /// <summary>
        /// Gets or sets the advert url.
        /// </summary>
        public string advert_url { get; set; }
        public bool? ispublished { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool? isdeleted { get; set; }
        public DateTime? deleted_at { get; set; }
        public virtual List<DocAdvert> DocAdverts { get; set; } = new List<DocAdvert>();
    }
}
