using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using System.Net.Mime;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Services;
using Microsoft.AspNetCore.Authorization;
using SAWSCore8API.Dtos;
using PayFast.AspNetCore;
using PayFast;
using DeviceDetectorNET;

namespace SAWSCore8API.Controllers
{
    /// <summary>
    /// Exposes subscription management and PayFast payment endpoints (create, update, cancel, query, and payment notifications).
    /// </summary>
    /// <remarks>Base route: <c>api/v1/Subscriptions</c>.</remarks>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {

        #region Fields

        private readonly SAWSDbContext _context;

        private readonly ISubscriptionService _subscriptionService;

        private ILogger<SubscriptionsController> _logger;
        private readonly IActivityLoggerService _activityLogger;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="SubscriptionsController"/> instance.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <param name="subscriptionService">Subscription service.</param>
        /// <param name="logger">Logger.</param>
        public SubscriptionsController(
            SAWSDbContext context,
            ISubscriptionService subscriptionService,
            ILogger<SubscriptionsController> logger,
            IActivityLoggerService activityLogger
            )
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _logger = logger;
            _activityLogger = activityLogger;
        }

        #endregion

        #region Subscriptions
        /// <summary>
        /// Creates a new subscription or updates an existing one based on the presence of <c>subscription.subscriptionId</c>.
        /// </summary>
        /// <param name="subscription">Subscription payload.</param>
        /// <returns>Result indicating success or failure with validation errors if any.</returns>
        /// <response code="200">Insert/update succeeded.</response>
        /// <response code="400">Validation or business rule failure.</response>
        /// <response code="404">Subscription to update not found.</response>
        [HttpPost("PostInsertSubscription")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Advert))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostInsertSubscription(Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            //log initialisation
            string clientIp = string.Empty;
            string userAgent = string.Empty;
            DeviceDetector dd = new DeviceDetector();
            ActivityLog alog = new ActivityLog();
            try
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                clientIp = remoteIp?.IsIPv4MappedToIPv6 == true
                                                    ? remoteIp.MapToIPv4().ToString()
                                                    : remoteIp?.ToString();

                userAgent = Request.Headers["User-Agent"].ToString();

                if (string.IsNullOrWhiteSpace(userAgent))
                    return BadRequest("User-Agent header is missing.");

                // Optional: Client Hints (modern browsers)
                var headers = Request.Headers.ToDictionary(
                    h => h.Key,
                    h => h.Value.ToString());

                var clientHints = ClientHints.Factory(headers);

                dd = new DeviceDetector(userAgent, clientHints);
                dd.Parse();
            }
            catch (Exception ex)
            {
                //
            }


            try
            {
                if (subscription.subscriptionId ==0)
                {
                    // Creating new subscription
                    var newSubscriptionResult = await _subscriptionService.CreateSubscription(subscription);

                    if (newSubscriptionResult.Success)
                    {
                        alog.activityLogId = 0;
                        alog.activityType = "subscription";
                        alog.remoteipaddress = clientIp;
                        alog.activityAction = "";//??
                        alog.activityDescription = "new subscription added";
                        alog.createdby_aspnetusername = "";//appUser.Username;//TODO:modify method to require Authorize and pull user info
                        alog.createdby_aspnetuserId = "";//loginResult.AspUserId;//TODO:modify method to require Authorize and pull user info
                        //device details
                        alog.UserAgent = userAgent;
                        alog.IsMobile = dd.IsMobile();
                        alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                        alog.Brand = dd.GetBrand() ?? "unknown";
                        alog.Model = dd.GetModel() ?? "unknown";
                        alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                        alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                        alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                        alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                        try
                        {
                            await _activityLogger.LogAsync(HttpContext, alog);
                        }
                        catch (Exception ex) 
                        { 
                            //nlog the issue
                        }

                        return Ok(newSubscriptionResult);
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert subscription. Invalid condition." } }
                            }
                    });
                }
                else
                {
                    // Updating existing subscription
                    if (!SubscriptionExists(subscription.subscriptionId))
                    {
                        return NotFound();
                    }

                    var newSubscriptionResult = await _subscriptionService.UpdateSubscription(subscription);

                    if (newSubscriptionResult.Success)
                    {
                        alog.activityLogId = 0;
                        alog.activityType = "subscription";
                        alog.remoteipaddress = clientIp;
                        alog.activityAction = "";//??
                        alog.activityDescription = "subscription updated";
                        alog.createdby_aspnetusername = "";//appUser.Username;//TODO:modify method to require Authorize and pull user info
                        alog.createdby_aspnetuserId = "";//loginResult.AspUserId;//TODO:modify method to require Authorize and pull user info
                        //device details
                        alog.UserAgent = userAgent;
                        alog.IsMobile = dd.IsMobile();
                        alog.DeviceType = dd.GetDeviceName() ?? "unknown";
                        alog.Brand = dd.GetBrand() ?? "unknown";
                        alog.Model = dd.GetModel() ?? "unknown";
                        alog.OsName = dd.GetOs().Match?.Name ?? "unknown";
                        alog.OsVersion = dd.GetOs().Match?.Version ?? "unknown";
                        alog.BrowserName = dd.GetClient().Match?.Name ?? "unknown";
                        alog.BrowserVersion = dd.GetClient().Match?.Version ?? "unknown";
                        try
                        {
                            await _activityLogger.LogAsync(HttpContext, alog);
                        }
                        catch (Exception ex)
                        {
                            //nlog the issue
                        }
                        return Ok(newSubscriptionResult);
                    }

                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to update subscription. Invalid condition." } }
                            }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionsController.PostInsertSubscription");
                return Problem("Unable to process the subscription.");
            }
        }

        /// <summary>
        /// Gets the currently active subscription for a user profile.
        /// </summary>
        /// <param name="id">User profile identifier.</param>
        /// <returns>Wrapper response containing the active subscription if found.</returns>
        /// <response code="200">Active subscription found.</response>
        /// <response code="404">No active subscription for user.</response>
        [HttpGet("GetActiveSubscriptionByUserProfileId")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Subscription))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetActiveSubscriptionByUserProfileId(int id)
        {
            try
            {
                var subscription = _subscriptionService.GetActiveSubscriptionByUserProfileId(id);
                if (subscription == null)
                {
                    return NotFound();
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Successfully returned active subscription",
                    DetailDescription = new
                    {
                        Subscription = subscription
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionController.GetActiveSubscriptionByUserProfileId");
                return Problem("Unable to get the active subscription");
            }
        }

        /// <summary>
        /// Gets a subscription by its identifier.
        /// </summary>
        /// <param name="id">Subscription identifier.</param>
        /// <returns>Subscription wrapped in a response object.</returns>
        /// <response code="200">Subscription found.</response>
        /// <response code="404">Subscription not found.</response>
        [HttpGet("GetSubscriptionById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Subscription))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSubscriptionById(int id)
        {
            try
            {
                var subscription = _subscriptionService.GetSubscriptionById(id);
                if (subscription == null)
                {
                    return NotFound();
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Successfully returned subscription",
                    DetailDescription = new
                    {
                        Subscription = subscription
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionController.GetSubscriptionById");
                return Problem("Unable to Get the subscription");
            }
        }

        /// <summary>
        /// Deletes a subscription by its identifier.
        /// </summary>
        /// <param name="id">Subscription identifier.</param>
        /// <returns>Deletion operation result.</returns>
        /// <response code="200">Subscription deleted.</response>
        /// <response code="404">Subscription not found.</response>
        /// <response code="400">Deletion failed.</response>
        [HttpDelete("DeleteSubscriptionById")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CreateResult))]
        // [Authorize(Roles.Administrator)]
        public async Task<IActionResult> DeleteSubscriptionById(int id)
        {
            try
            {
                if (!SubscriptionExists(id))
                {
                    return NotFound();
                }

                var result = await _subscriptionService.DeleteSubscriptionById(id);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return new BadRequestResult();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionController.DeleteSubscriptionById");
                return Problem("Unable to Delete the subscription");
            }
        }

        #endregion

        #region PayFast
        /// <summary>
        /// Initiates a recurring PayFast payment and returns redirect/processing URL.
        /// </summary>
        /// <param name="request">Payment request details.</param>
        /// <returns>Result with redirect URL or validation errors.</returns>
        /// <response code="200">Recurring payment initiated.</response>
        /// <response code="400">Validation or initiation failed.</response>
        [HttpPost("RecurringPayment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        public async Task<IActionResult> RecurringPayment([FromBody] Payment request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            try
            {
                var newRecurringPaymentResult = await _subscriptionService.RecuringPayment(request);

                if (newRecurringPaymentResult.Success)
                {
                    return Ok(newRecurringPaymentResult);
                }

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert fayfast recurring subscription. Invalid condition." } }
                            }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionsController.RecurringPayment");
                return Problem("Unable to process recuring payment.");
            }
        }

        /// <summary>
        /// Initiates a once-off PayFast payment.
        /// </summary>
        /// <param name="request">Payment request details.</param>
        /// <returns>Result with redirect URL or validation errors.</returns>
        /// <response code="200">Payment initiated.</response>
        /// <response code="400">Validation or initiation failed.</response>
        [HttpPost("OnceOffPayment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        public async Task<IActionResult> OnceOffPayment([FromBody] Payment request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            try
            {
                var newOnceOffPaymentResult = await _subscriptionService.OnceOffPayment(request);

                if (newOnceOffPaymentResult.Success)
                {
                    return Ok(newOnceOffPaymentResult);
                }

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert fayfast once-off payment. Invalid condition." } }
                            }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionsController.OnceOffPayment");
                return Problem("Unable to process once-off payment.");
            }
        }

        /// <summary>
        /// Initiates an ad-hoc PayFast payment.
        /// </summary>
        /// <param name="request">Payment request details.</param>
        /// <returns>Result indicating success or failure.</returns>
        /// <response code="200">Payment initiated.</response>
        /// <response code="400">Validation or initiation failed.</response>
        [HttpPost("AdHocPayment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CreateResult))]
        public async Task<IActionResult> AdHocPayment([FromBody] Payment request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).AsEnumerable()
                );

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = errorMessages
                });
            }

            try
            {
                var newAdhocPaymentResult = await _subscriptionService.AdHocPayment(request);

                if (newAdhocPaymentResult.Success)
                {
                    return Ok(newAdhocPaymentResult);
                }

                return BadRequest(new CreateResult
                {
                    Success = false,
                    ErrorMessages = new Dictionary<string, IEnumerable<string>>
                            {
                                { "General", new[] { "Failed to insert fayfast adhoc payment. Invalid condition." } }
                            }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionsController.AdHocPayment");
                return Problem("Unable to process adhoc payment.");
            }
        }

        /// <summary>
        /// Handles PayFast Instant Transaction Notification (ITN).
        /// </summary>
        /// <param name="payFastNotifyViewModel">Notification model bound by <see cref="PayFastNotifyModelBinder"/>.</param>
        /// <returns>Result indicating success or failure of notification processing.</returns>
        /// <response code="200">Notification processed.</response>
        /// <response code="400">Invalid notification payload.</response>
        [HttpPost("Notify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Notify([ModelBinder(BinderType = typeof(PayFastNotifyModelBinder))] PayFastNotify payFastNotifyViewModel)
        {
            if (payFastNotifyViewModel == null)
            {
                return new BadRequestObjectResult("Invalid PayFast notification received.");
            }

            try
            {
                var newNotifyResult = await _subscriptionService.NotifyITN(payFastNotifyViewModel);

                if (newNotifyResult.Success)
                {
                    return new OkObjectResult(newNotifyResult);
                }
                else
                {
                    return new BadRequestObjectResult(newNotifyResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionsController.Notify");
                return Problem("Unable to process payment notification.");
            }
        }

        /// <summary>
        /// Test endpoint for PayFast notification binding.
        /// </summary>
        /// <param name="payFastNotifyViewModel">Notification model.</param>
        /// <returns>Simple acknowledgement for test purposes.</returns>
        /// <response code="200">Binding test successful.</response>
        /// <response code="400">Invalid payload.</response>
        [HttpPost("NotifyTest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NotifyTest([ModelBinder(BinderType = typeof(PayFastNotifyModelBinder))] PayFastNotify payFastNotifyViewModel)
        {
            if (payFastNotifyViewModel == null)
            {
                return new BadRequestObjectResult("Invalid PayFast notification received.");
            }
            return new OkObjectResult("Notify testing");
        }

        /// <summary>
        /// Cancels an existing active subscription and provisions a free fallback subscription.
        /// </summary>
        /// <param name="subscriptionId">Identifier of the subscription to cancel.</param>
        /// <returns>Result of cancellation sequence.</returns>
        /// <response code="200">Cancellation succeeded.</response>
        /// <response code="404">Subscription not found.</response>
        /// <response code="400">Cancellation failed.</response>
        [HttpPost("CancelSubscription")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelSubscription(int subscriptionId)
        {
            try
            {
                var activeSub = _subscriptionService.GetSubscriptionById(subscriptionId);

                if (activeSub == null)
                {
                    return new NotFoundObjectResult("Subscription not found");
                }

                var activeSubscription = await _subscriptionService.CancelSubscription(activeSub.subscription_token);

                activeSub.isactive = false;
                var userId = activeSub.userprofileid;

                if (activeSubscription.Success)
                {
                    var updateSub = await _subscriptionService.UpdateSubscription(activeSub);

                    if (updateSub.Success)
                    { 
                        var addFreeSub = await _subscriptionService.CreateFreeSubscription(userId);

                        if (addFreeSub.Success)
                        {
                            return new OkObjectResult(activeSubscription);
                        }
                    }
                }
                return new BadRequestObjectResult("No active subscription to cancel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception from SubscriptionsController.CancelSubscription");
                return Problem("Unable to process cancel payment.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a subscription exists by identifier.
        /// </summary>
        /// <param name="id">Subscription identifier.</param>
        /// <returns>True if exists; otherwise false.</returns>
        private bool SubscriptionExists(int id)
        {
            return _context.Subscriptions.Any(e => e.subscriptionId == id);
        }

        #endregion

    }
}
