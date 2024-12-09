#pragma warning disable CS1591
using Microsoft.AspNetCore.Identity;

namespace SAWSCore8API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; }
        public bool IsAdminUser { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PhotoUrl { get; set; }
    }
}

#pragma warning restore CS1591