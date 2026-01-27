using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

using PayFast;
using PayFast.AspNetCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Web;
using SAWSCore8API.Options;

namespace SAWSCore8API.Services
{
    /// <summary>
    /// Implements subscription management and PayFast payment operations.
    /// Handles creating/updating/deleting subscriptions, initiating payments,
    /// processing PayFast ITN notifications, and cancelling active subscriptions.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private ILogger<SubscriptionService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly PayFastSettings payFastSettings;
        private readonly HttpClient _httpClient;
        private readonly PayFastOptions _payFastOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionService"/> class.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <param name="uriService">URI helper service for pagination or link building.</param>
        /// <param name="httpContextAccessor">HTTP context accessor.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="configuration">Application configuration provider.</param>
        /// <param name="payFastSettings">PayFast configuration options.</param>
        /// <param name="httpClient">HTTP client used for external calls.</param>
        /// <param name="payFastOptions">PayFast options (public base URL).</param>
        public SubscriptionService(SAWSDbContext context, IUriService uriService, IHttpContextAccessor httpContextAccessor, ILogger<SubscriptionService> logger, IConfiguration configuration, IOptions<PayFastSettings> payFastSettings, HttpClient httpClient, IOptions<PayFastOptions> payFastOptions)
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
            this.payFastSettings = payFastSettings.Value;
            _httpClient = httpClient ?? new HttpClient();
            _payFastOptions = payFastOptions.Value;
        }

        private bool TryGetPublicBaseUri(out Uri publicBaseUri, out string error)
        {
            publicBaseUri = default!;
            error = string.Empty;

            var raw = _payFastOptions.PublicBaseUrl;
            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "PayFast PublicBaseUrl is not configured. Please set PayFast:PublicBaseUrl to a publicly reachable https:// URL.";
                return false;
            }

            if (!Uri.TryCreate(raw, UriKind.Absolute, out publicBaseUri))
            {
                error = "PayFast PublicBaseUrl is invalid. Please set PayFast:PublicBaseUrl to a valid absolute https:// URL.";
                return false;
            }

            if (!string.Equals(publicBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                error = "PayFast PublicBaseUrl must use https:// (PayFast rejects non-HTTPS return/cancel URLs).";
                return false;
            }

            return true;
        }

        private Uri BuildPublicUrl(Uri publicBaseUri, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return publicBaseUri;
            }

            var baseString = publicBaseUri.ToString().TrimEnd('/');
            var relString = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
            return new Uri(baseString + relString, UriKind.Absolute);
        }

        /// <summary>
        /// Creates a new subscription record.
        /// </summary>
        /// <param name="subscription">Subscription to create.</param>
        /// <returns>A result containing the new identifier on success.</returns>
        public Task<CreateResult> CreateSubscription(Subscription subscription)
        {
            subscription.created_at = DateTime.Now;
            subscription.updated_at = DateTime.Now;
            subscription.isdeleted = false;

            _context.Subscriptions.Add(subscription);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(subscription.subscriptionId));
        }

        /// <summary>
        /// Creates a free subscription for the specified user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Create result with the new subscription identifier.</returns>
        public Task<CreateResult> CreateFreeSubscription(int userId)
        {

            var user = _context.userProfiles.First(a => a.userprofileid == userId);

            if (user == null)
            {
                return Task.FromResult(CreateResult.FailureResult("User is not found"));
            }

            Subscription freeSubscription = new Subscription();

            freeSubscription.userprofileid = userId;
            freeSubscription.package_name = user.userrole == "Admin" ? "Admin" : "monthly Free";
            freeSubscription.package_id =1;
            freeSubscription.package_price =0;
            freeSubscription.start_date = DateTime.Now;
            freeSubscription.end_date = DateTime.Now.AddYears(1);
            freeSubscription.subscription_duration =365;
            freeSubscription.subscription_token = "";
            freeSubscription.isactive = true;
            freeSubscription.created_at = DateTime.Now;
            freeSubscription.updated_at = DateTime.Now;
            freeSubscription.isdeleted = false;

            _context.Subscriptions.Add(freeSubscription);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(freeSubscription.subscriptionId));
        }

        /// <summary>
        /// Initiates a PayFast recurring subscription payment and returns a redirect URL.
        /// </summary>
        /// <param name="request">Payment request details.</param>
        /// <returns>Result with the redirect URL or an error message.</returns>
        public Task<CreateSubscriptionResult> RecuringPayment(Payment request)
        {
            if (request != null)
            {
                if (!TryGetPublicBaseUri(out var publicBaseUri, out var error))
                {
                    _logger.LogError("{Error}", error);
                    return Task.FromResult(CreateSubscriptionResult.FailureResult(error));
                }

                var passphrase = _configuration.GetValue<string>("payFast:passphrase");
                var recurringRequest = new PayFastRequest(passphrase);

                // Merchant Details
                recurringRequest.merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
                recurringRequest.merchant_key = _configuration.GetValue<string>("payFast:merchant_key");

                // Always use backend-generated HTTPS return/cancel URLs
                recurringRequest.return_url = BuildPublicUrl(publicBaseUri, "/v1/subscriptions/payfast/return").ToString();
                recurringRequest.cancel_url = BuildPublicUrl(publicBaseUri, "/v1/subscriptions/payfast/cancel").ToString();

                // Keep notify_url as server-to-server callback endpoint (caller should supply API notify URL)
                recurringRequest.notify_url = request.notifyUrl;

                // Additional Details
                recurringRequest.custom_int1 = request.userId;
                recurringRequest.custom_int2 = request.package_id;
                recurringRequest.custom_int3 = request.subscription_amount;
                recurringRequest.custom_str1 = request.package_name;
                recurringRequest.custom_str2 = request.subscription_type;

                // Buyer Details
                recurringRequest.email_address = request.email_address;
                recurringRequest.name_first = request.name_first;
                recurringRequest.name_last = request.name_last;

                // Transaction Details
                recurringRequest.m_payment_id = request.m_payment_id;
                recurringRequest.amount = request.amount;
                recurringRequest.item_name = request.item_name;
                recurringRequest.item_description = request.item_description;

                // Transaction Options
                recurringRequest.email_confirmation = request.email_confirmation;
                recurringRequest.confirmation_address = request.confirmation_email;

                // Recurring Billing Details
                recurringRequest.subscription_type = SubscriptionType.Subscription;
                recurringRequest.billing_date = DateTime.Now;
                recurringRequest.recurring_amount = request.recurring_amount;
                recurringRequest.cycles = 0;

                if (request.frequency.ToLower() == "monthly")
                {
                    recurringRequest.frequency = BillingFrequency.Monthly;
                }
                else
                {
                    recurringRequest.frequency = BillingFrequency.Annual;
                }

                var redirectUrl = $"{this.payFastSettings.ProcessUrl}{recurringRequest.ToString()}";

                var redirectLink = _configuration.GetValue<string>("payFast:endPoint") + "?" + redirectUrl;

                return Task.FromResult(CreateSubscriptionResult.SuccessResult(redirectLink));
            }
            else
            {
                return Task.FromResult(CreateSubscriptionResult.FailureResult("Payfast recurring request is empty"));
            }
        }

        /// <summary>
        /// Initiates a PayFast once-off payment and returns a redirect URL.
        /// </summary>
        /// <param name="request">Payment request details.</param>
        /// <returns>Result with the redirect URL or an error message.</returns>
        public Task<CreateSubscriptionResult> OnceOffPayment(Payment request)
        {
            if (request != null)
            {
                if (!TryGetPublicBaseUri(out var publicBaseUri, out var error))
                {
                    _logger.LogError("{Error}", error);
                    return Task.FromResult(CreateSubscriptionResult.FailureResult(error));
                }

                var passphrase = _configuration.GetValue<string>("payFast:passphrase");
                var onceOffRequest = new PayFastRequest(passphrase);

                // Merchant Details
                onceOffRequest.merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
                onceOffRequest.merchant_key = _configuration.GetValue<string>("payFast:merchant_key");

                // Always use backend-generated HTTPS return/cancel URLs
                onceOffRequest.return_url = BuildPublicUrl(publicBaseUri, "/v1/subscriptions/payfast/return").ToString();
                onceOffRequest.cancel_url = BuildPublicUrl(publicBaseUri, "/v1/subscriptions/payfast/cancel").ToString();

                // Keep notify_url as server-to-server callback endpoint (caller should supply API notify URL)
                onceOffRequest.notify_url = request.notifyUrl;

                // Buyer Details
                onceOffRequest.email_address = request.email_address;
                onceOffRequest.name_first = request.name_first;
                onceOffRequest.name_last = request.name_last;

                // Transaction Details
                onceOffRequest.m_payment_id = request.m_payment_id;
                onceOffRequest.amount = request.amount;
                onceOffRequest.item_name = request.item_name;
                onceOffRequest.item_description = request.item_description;

                // Transaction Options
                onceOffRequest.email_confirmation = request.email_confirmation;
                onceOffRequest.confirmation_address = request.confirmation_email;

                var redirectUrl = $"{this.payFastSettings.ProcessUrl}{onceOffRequest.ToString()}";

                var redirectLink = _configuration.GetValue<string>("payFast:endPoint") + "?" + redirectUrl;

                return Task.FromResult(CreateSubscriptionResult.SuccessResult(redirectLink));
            }
            else
            {
                return Task.FromResult(CreateSubscriptionResult.FailureResult("Payfast once-off request is empty"));
            }
        }

        /// <summary>
        /// Initiates a PayFast ad-hoc payment and returns a redirect URL.
        /// </summary>
        /// <param name="request">Payment request details.</param>
        /// <returns>Result with the redirect URL or an error message.</returns>
        public Task<CreateSubscriptionResult> AdHocPayment(Payment request)
        {
            if (request != null)
            {
                if (!TryGetPublicBaseUri(out var publicBaseUri, out var error))
                {
                    _logger.LogError("{Error}", error);
                    return Task.FromResult(CreateSubscriptionResult.FailureResult(error));
                }

                var passphrase = _configuration.GetValue<string>("payFast:passphrase");
                var adHocRequest = new PayFastRequest(passphrase);

                // Merchant Details
                adHocRequest.merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
                adHocRequest.merchant_key = _configuration.GetValue<string>("payFast:merchant_key");

                // Always use backend-generated HTTPS return/cancel URLs
                adHocRequest.return_url = BuildPublicUrl(publicBaseUri, "/v1/subscriptions/payfast/return").ToString();
                adHocRequest.cancel_url = BuildPublicUrl(publicBaseUri, "/v1/subscriptions/payfast/cancel").ToString();

                // Keep notify_url as server-to-server callback endpoint (caller should supply API notify URL)
                adHocRequest.notify_url = request.notifyUrl;

                // Buyer Details
                adHocRequest.email_address = request.email_address;
                adHocRequest.name_first = request.name_first;
                adHocRequest.name_last = request.name_last;

                // Transaction Details
                adHocRequest.m_payment_id = request.m_payment_id;
                adHocRequest.amount = request.amount;
                adHocRequest.item_name = request.item_name;
                adHocRequest.item_description = request.item_description;

                // Transaction Options
                adHocRequest.email_confirmation = request.email_confirmation;
                adHocRequest.confirmation_address = request.confirmation_email;

                // Recurring Billing Details
                adHocRequest.subscription_type = SubscriptionType.AdHoc;

                var redirectUrl = $"{this.payFastSettings.ProcessUrl}{adHocRequest.ToString()}";

                var redirectLink = _configuration.GetValue<string>("payFast:endPoint") + "?" + redirectUrl;

                return Task.FromResult(CreateSubscriptionResult.SuccessResult(redirectLink));
            }
            else
            {
                return Task.FromResult(CreateSubscriptionResult.FailureResult("Payfast adhoc request is empty"));
            }
        }

        /// <summary>
        /// Processes PayFast Instant Transaction Notification (ITN) messages.
        /// </summary>
        /// <param name="payFastNotify">Notification payload from PayFast.</param>
        /// <returns>Result indicating the processing outcome of the ITN.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the notification is null.</exception>
        public async Task<NotifyResult> NotifyITN(PayFastNotify payFastNotify)
        {
            if (payFastNotify == null)
            {
                throw new ArgumentNullException(nameof(payFastNotify), "PayFast notification is null");
            }

            switch (payFastNotify.payment_status)
            {
                case "COMPLETE":
                    return await HandleSuccessfulPayment(payFastNotify);
                case "FAILED":
                    return await HandleFailedPayment();
                case "PENDING":
                    return await HandlePendingPayment();
                default:
                    // Handle any other statuses
                    return NotifyResult.FailureResult("Failed to get notification status");
            }
        }

        /// <summary>
        /// Cancels an active PayFast subscription using the provided token.
        /// </summary>
        /// <param name="token">PayFast subscription token.</param>
        /// <returns>Cancellation result, including a message.</returns>
        public async Task<CancelResult> CancelSubscription(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return CancelResult.FailureResult("No token provided");
            }

            this.payFastSettings.MerchantId = _configuration.GetValue<string>("payFast:merchant_id");
            this.payFastSettings.MerchantKey = _configuration.GetValue<string>("payFast:merchant_key");
            // this.payFastSettings.NotifyUrl = _configuration.GetValue<string>("payFast:NotifyUrl");
            this.payFastSettings.PassPhrase = _configuration.GetValue<string>("payFast:passphrase");
            bool istesting = _configuration.GetValue<bool>("payFast:isTesting");

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.payfast.co.za/")
            };

            var subscriptionCancellation = new PayFastIntegrationClient(client, Microsoft.Extensions.Options.Options.Create(payFastSettings));

            var result = await subscriptionCancellation.Cancel(token, istesting);

            if (result.status != "success")
            {
                return CancelResult.FailureResult("Did not cancel payfast subscription");
            }

            return CancelResult.SuccessResult("Successfully cancelled subscription");
        }

        /// <summary>
        /// Updates an existing subscription record.
        /// </summary>
        /// <param name="subscription">Subscription to update.</param>
        /// <returns>Update operation result.</returns>
        public Task<UpdateResult> UpdateSubscription(Subscription subscription)
        {
            subscription.updated_at = DateTime.Now;
            subscription.isdeleted = false;

            _context.Subscriptions.Update(subscription);
            Save();

            return Task.FromResult(UpdateResult.SuccessResultUpdate(subscription.subscriptionId));
        }

        /// <summary>
        /// Gets a subscription by its identifier.
        /// </summary>
        /// <param name="id">Subscription identifier.</param>
        /// <returns>The subscription if found; otherwise throws if not present.</returns>
        public Subscription GetSubscriptionById(int id)
        {
            return _context.Subscriptions
                .Where(d => d.subscriptionId == id)
                .First();
        }

        /// <summary>
        /// Gets the active subscription for a user profile.
        /// </summary>
        /// <param name="userId">User profile identifier.</param>
        /// <returns>The active subscription if found; otherwise throws if not present.</returns>
        public Subscription GetActiveSubscriptionByUserProfileId(int userId)
        {
            return _context.Subscriptions
                .Where(d => d.userprofileid == userId && d.isactive)
                .First();
        }

        /// <summary>
        /// Soft-deletes a subscription by setting deletion flags and timestamps.
        /// </summary>
        /// <param name="id">Subscription identifier.</param>
        /// <returns>Deletion result indicating success or failure.</returns>
        public Task<DeleteResult> DeleteSubscriptionById(int id)
        {
            var subscription = _context.Subscriptions.First(a => a.subscriptionId == id);

            if (subscription != null)
            {
                subscription.isdeleted = true;
                subscription.deleted_at = DateTime.Now;

                Save();
                return Task.FromResult(DeleteResult.SuccessResult("Successfully deleted subscription"));
            }
            else
            {
                return Task.FromResult(DeleteResult.FailureResult("Failed to delete subscription"));
            }
        }

        /// <summary>
        /// Handles ITN for successful payments by creating a new subscription and deactivating the previous one.
        /// </summary>
        /// <param name="payFastNotify">PayFast notification.</param>
        /// <returns>Notification result indicating success or failure.</returns>
        private async Task<NotifyResult> HandleSuccessfulPayment(PayFastNotify payFastNotify)
        {
            if (!int.TryParse(payFastNotify.custom_int1, out int userId) ||
               !int.TryParse(payFastNotify.custom_int2, out int packageId) ||
               !int.TryParse(payFastNotify.custom_int3, out int packagePrice)
               )
            {
                return NotifyResult.FailureResult("Invalid integer value in custom fields");
            }

            var newSubscription = new Subscription
            {
                subscriptionId =0,
                userprofileid = userId,
                package_name = payFastNotify.custom_str1,
                package_id = packageId,
                package_price = packagePrice,
                start_date = DateTime.Now,
                end_date = DateTime.Now.AddMonths(12),
                subscription_duration =365,
                subscription_token = payFastNotify.token,
                isactive = true
            };

            var activeSubscriptionResult = GetActiveSubscriptionByUserProfileId(userId);

            if (activeSubscriptionResult == null)
            {
                var newSubscriptionResult = await CreateSubscription(newSubscription);

                if (newSubscriptionResult.Success)
                {
                    return NotifyResult.SuccessResult("Successfully added new subscription");
                }
                else
                {
                    return NotifyResult.FailureResult("Failed to add new subscription");
                }
            }

            // Cancel PayFast
            var cancelActiveSubscriptionResult = await CancelSubscription(activeSubscriptionResult.subscription_token);

            if (cancelActiveSubscriptionResult.Success)
            {
                // Paid active subscription

                var newSubscriptionResult = await CreateSubscription(newSubscription);

                if (newSubscriptionResult.Success)
                {
                    // Update existing subscription
                    activeSubscriptionResult.isactive = false;

                    var updateSub = await UpdateSubscription(activeSubscriptionResult);

                    if (updateSub.Success)
                    {
                        return NotifyResult.SuccessResult("Successfully updated paid subscription");
                    }
                }
                return NotifyResult.FailureResult("Failed to update existing subscription");
            }
            else if (!cancelActiveSubscriptionResult.Success && activeSubscriptionResult != null)
            {
                // Free active subscription

                var newSubscriptionResult = await CreateSubscription(newSubscription);

                if (newSubscriptionResult.Success)
                {
                    // Update existing subscription
                    activeSubscriptionResult.isactive = false;

                    var updateSub = await UpdateSubscription(activeSubscriptionResult);

                    if (updateSub.Success)
                    {
                        return NotifyResult.SuccessResult("Successfully updated free subscription");
                    }
                }
                return NotifyResult.FailureResult("Failed to update existing subscription");
            }
            else
            {
                return NotifyResult.FailureResult("Failed to perform subscriptions addition to database");
            }
        }

        /// <summary>
        /// Handles failed payment ITN messages.
        /// </summary>
        /// <returns>Failure notification result.</returns>
        private Task<NotifyResult> HandleFailedPayment()
        {
            return Task.FromResult(NotifyResult.FailureResult("Failed to add subscription"));
        }

        /// <summary>
        /// Handles pending payment ITN messages.
        /// </summary>
        /// <returns>Pending notification result.</returns>
        private Task<NotifyResult> HandlePendingPayment()
        {
            return Task.FromResult(NotifyResult.FailureResult("Pending adding of subscription"));
        }

        /// <summary>
        /// Persists changes to the database.
        /// </summary>
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
