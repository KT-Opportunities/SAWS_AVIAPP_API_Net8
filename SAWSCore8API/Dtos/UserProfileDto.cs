using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

using SAWSCore8API.Models;

namespace SAWSCore8API.Dtos
{
    public class UserProfileDto
    {
        public int userprofileid { get; set; }
        public string fullname { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string userrole { get; set; }
        public string aspuid { get; set; }
        public bool? isactive { get; set; }
        public DateTime? created_at { get; set; }
        public virtual List<SubscriptionDto> Subscription { get; set; } = new List<SubscriptionDto>();

    }
}
