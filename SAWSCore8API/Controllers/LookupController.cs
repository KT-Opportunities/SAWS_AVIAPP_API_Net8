using Microsoft.AspNetCore.Mvc;
using SAWSCore8API.Models;
using SAWSCore8API.Interfaces;
using SAWSCore8API.DbContexts;

namespace SAWSCore8API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {

        #region Fields
        private readonly ILookupService _lookupService;
        private ILogger<LookupController> _logger;
        public IConfiguration _configuration { get; }

        #endregion

        #region Constructors

        public LookupController(
                    ILookupService lookupService,
        ILogger<LookupController> logger,
        IConfiguration configuration
            )
        {
            _lookupService = lookupService;
            _logger = logger;
            _configuration = configuration;
        }

        #endregion

        #region Lookup

        [HttpGet("GetRegistrationsPerUserType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetRegistrationsPerUserType()
        {
            var startDate = DateTime.Now.AddMonths(-12);
            var monthNames = new[]
            {
                    "January", "February", "March", "April", "May", "June",
                    "July", "August", "September", "October", "November", "December"
            };

            try
            {
                var records = _lookupService.GetRegistrationsPerUserType();

                var result = records
                        .GroupBy(r => new { r.year, r.month })
                        .Select(g => new
                        {
                            MonthString = monthNames[g.Key.month - 1],
                            Month = g.Key.month,
                            Year = g.Key.year,
                            UserTypes = g.Select(r => new
                            {
                                r.userRole,
                                r.subscriptionType,
                                r.registrations
                            }).ToList()
                        })
                        .OrderBy(g => g.Year)
                        .ThenBy(g => g.Month)
                        .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from LookupController.GetRegistrationsPerUserType");
                return Problem("Unable to process GetRegistrationsPerUserType.");
            }
        }

        [HttpGet("GetSubscriptionsPerPackageType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetSubscriptionsPerPackageType()
        {
            try
            {
                var userSubscriptionsCounts = _lookupService.GetSubscriptionsPerPackageType();

                var newUserSubscriptionsCounts = userSubscriptionsCounts.Where(g => !(g.userRole == "Subscriber" && g.subscriptionType == null));
                var totalCount = newUserSubscriptionsCounts.Sum(usc => usc.subscriptions);

                var response = new
                {
                    userSubscriptionCounts = newUserSubscriptionsCounts,
                    totalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from LookupController.GetSubscriptionsPerPackageType");
                return Problem("Unable to process GetSubscriptionsPerPackageType.");
            }
        }

        [HttpGet("GetAdvertsClickPerMonth")]
        [MapToApiVersion("1")]
        public IActionResult GetAdvertsClickPerMonth()
        {
            try
            {
                var result = _lookupService.GetAdvertsClickPerMonth();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from LookupController.GetAdvertsClickPerMonth");
                return Problem("Unable to process GetAdvertsClickPerMonth.");
            }
        }

        #endregion
    }
}
