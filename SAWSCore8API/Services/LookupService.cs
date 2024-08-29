using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SAWSCore8API.Controllers;

namespace SAWSCore8API.Services
{
    public class LookupService : ILookupService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private ILogger<LookupService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LookupService(SAWSDbContext context, IUriService uriService, IHttpContextAccessor httpContextAccessor, ILogger<LookupService> logger)
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IEnumerable<UserRegistrationResult> GetRegistrationsPerUserType()
        {
            var startDate = DateTime.Now.AddMonths(-12);

            var result = _context.userProfiles
                        .Where(u => u.created_at >= startDate)
                        .GroupJoin(
                            _context.Subscriptions,
                            userProfile => userProfile.userprofileid,
                            subscription => subscription.userprofileid,
                            (userProfile, subscriptions) => new { userProfile, subscriptions }
                        )
                        .SelectMany(
                            us => us.subscriptions.DefaultIfEmpty(),
                            (us, subscription) => new
                            {
                                us.userProfile,
                                SubscriptionType = us.userProfile.userrole == "Subscriber" && subscription != null
                                    ? (subscription.package_name.Contains("Regulated") || subscription.package_name.Contains("Premium") ? "Subscribed"
                                        : subscription.package_name.Contains("Free") ? "Free"
                                        : null)
                                    : us.userProfile.userrole == "Admin"
                                        ? "Admin"
                                        : null
                            }
                        )
                        .GroupBy(u => new
                        {
                            u.userProfile.userrole,
                            Month = u.userProfile.created_at.Value.Month,
                            Year = u.userProfile.created_at.Value.Year,
                            SubscriptionType = u.SubscriptionType
                        })
                        .Select(g => new UserRegistrationResult
                        {
                            userRole = g.Key.userrole,
                            subscriptionType = g.Key.SubscriptionType,
                            month = g.Key.Month,
                            year = g.Key.Year,
                            registrations = g.Count()
                        })
                        .ToList();


            return result;
        }

        public IEnumerable<SubscriptionsTypeResult> GetSubscriptionsPerPackageType()
        {

            var result = _context.userProfiles
                        // .Where(u => u.isdeleted == false)
                        .GroupJoin(
                            _context.Subscriptions,
                            userProfile => userProfile.userprofileid,
                            subscription => subscription.userprofileid,
                            (userProfile, subscriptions) => new { userProfile, subscriptions }
                        )
                        .SelectMany(
                            us => us.subscriptions.DefaultIfEmpty(),
                            (us, subscription) => new
                            {
                                us.userProfile,
                                SubscriptionType = us.userProfile.userrole == "Subscriber" && subscription != null
                                    ? (subscription.package_name.Contains("Regulated") || subscription.package_name.Contains("Premium") ? "Subscribed"
                                        : subscription.package_name.Contains("Free") && subscription.isactive ? "Free"
                                        : null)
                                    : us.userProfile.userrole == "Admin"
                                        ? "Admin"
                                        : null
                            }
                        )
                        .GroupBy(u => new
                        {
                            u.userProfile.userrole,
                            SubscriptionType = u.SubscriptionType
                        })
                        .Select(g => new SubscriptionsTypeResult
                        {
                            userRole = g.Key.userrole,
                            subscriptionType = g.Key.SubscriptionType,
                            subscriptions = g.Count()
                        })
                        .ToList();

            return result;
        }

        public IEnumerable<AdvertClicksResult> GetAdvertsClickPerMonth()
        {
            var startDate = DateTime.Now.AddMonths(-12);
            var monthNames = new[]
           {
                    "January", "February", "March", "April", "May", "June",
                    "July", "August", "September", "October", "November", "December"
            };

            var result = _context.AdvertClicks
               .Where(u => u.created_at >= startDate && u.isdeleted == false)
               .GroupBy(u => new { Month = u.created_at.Value.Month, Year = u.created_at.Value.Year })
               .Select(g => new AdvertClicksResult
               {
                   monthString = monthNames[g.Key.Month - 1],
                   month = g.Key.Month,
                   year = g.Key.Year,
                   clicks = g.Count()
               })
               .ToList();

            return result;
        }

    }
}
