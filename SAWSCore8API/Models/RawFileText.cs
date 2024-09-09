using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class RawTextFile
    {
        public string foldername { get; set; }
        public string filename { get; set; }
        public DateTime? lastmodified { get; set; }
        public string filecontent { get; set; }
    }
}
