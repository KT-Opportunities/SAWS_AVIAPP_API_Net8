using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class GetTextFile : TextFile
    {
        public string filetextcontent { get; set; }
    }
}
