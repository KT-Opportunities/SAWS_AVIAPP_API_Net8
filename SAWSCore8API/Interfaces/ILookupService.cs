using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface ILookupService
    {
        IEnumerable<UserRegistrationResult> GetRegistrationsPerUserType();
        IEnumerable<SubscriptionsTypeResult> GetSubscriptionsPerPackageType();
        IEnumerable<AdvertClicksResult> GetAdvertsClickPerMonth();
    }
}