using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAWSCore8API.Models
{
    [NotMapped]
    public class GetRawFile
    {
        public string foldername { get; set; }
        public string filename { get; set; }
        public DateTime? lastmodified { get; set; }
        public string filecontent { get; set; }

       public static GetRawFile Result(string foldername, string filename, DateTime lastmodified, string filecontent)
        {
            return new GetRawFile
            {
                foldername = foldername,
                filename = filename,
                lastmodified = lastmodified,
                filecontent = filecontent,
            };
        }


    }
}
