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

namespace SAWSCore8API.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private ILogger<SubscriptionService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly PayFastSettings payFastSettings;
        private readonly HttpClient _httpClient;

        public SubscriptionService(SAWSDbContext context, IUriService uriService, IHttpContextAccessor httpContextAccessor, ILogger<SubscriptionService> logger, IConfiguration configuration, IOptions<PayFastSettings> payFastSettings, HttpClient httpClient)
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
            this.payFastSettings = payFastSettings.Value;
            _httpClient = httpClient ?? new HttpClient();
        }

        public Task<CreateResult> CreateSubscription(Subscription subscription)
        {
            subscription.created_at = DateTime.Now;
            subscription.updated_at = DateTime.Now;
            subscription.isdeleted = false;

            _context.Subscriptions.Add(subscription);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(subscription.subscriptionId));
        }

        public Task<CreateSubscriptionResult> RecuringPayment(Payment request)
        {
            if (request != null)
            {
                var passphrase = _configuration.GetValue<string>("payFast:passphrase");
                var recurringRequest = new PayFastRequest(passphrase);

                // Merchant Details
                recurringRequest.merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
                recurringRequest.merchant_key = _configuration.GetValue<string>("payFast:merchant_key");
                recurringRequest.return_url = request.returnUrl;
                recurringRequest.cancel_url = request.cancelUrl;
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

        public Task<CreateSubscriptionResult> OnceOffPayment(Payment request)
        {
            if (request != null)
            {
                var passphrase = _configuration.GetValue<string>("payFast:passphrase");
                var onceOffRequest = new PayFastRequest(passphrase);

                // Merchant Details
                onceOffRequest.merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
                onceOffRequest.merchant_key = _configuration.GetValue<string>("payFast:merchant_key");
                onceOffRequest.return_url = request.returnUrl;
                onceOffRequest.cancel_url = request.cancelUrl;
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

        public Task<CreateSubscriptionResult> AdHocPayment(Payment request)
        {
            if (request != null)
            {
                var passphrase = _configuration.GetValue<string>("payFast:passphrase");
                var adHocRequest = new PayFastRequest(passphrase);

                // Merchant Details
                adHocRequest.merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
                adHocRequest.merchant_key = _configuration.GetValue<string>("payFast:merchant_key");
                adHocRequest.return_url = request.returnUrl;
                adHocRequest.cancel_url = request.cancelUrl;
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
                    return NotifyResult.FailureResult("Failed to get notifucation status");
            }
        }

        public async Task<UpdateResult> CancelSubscription(string token)
        {
            /* var activeSubscription = GetSubscriptionById(subId);

             if (activeSubscription == null)
             {
                 return UpdateResult.FailureResult("No active subscription");
             }*/

            /*activeSubscription.updated_at = DateTime.Now;
            activeSubscription.isdeleted = false;
            activeSubscription.isactive = false;*/

            /*if (activeSubscription.subscription_token != "")
            {*/
            this.payFastSettings.MerchantId = _configuration.GetValue<string>("payFast:merchant_id");
            this.payFastSettings.MerchantKey = _configuration.GetValue<string>("payFast:merchant_key");
            this.payFastSettings.NotifyUrl = _configuration.GetValue<string>("payFast:NotifyUrl");
            this.payFastSettings.PassPhrase = _configuration.GetValue<string>("payFast:passphrase");
            bool istesting = _configuration.GetValue<bool>("payFast:isTesting");

            var merchant_id = _configuration.GetValue<string>("payFast:merchant_id");
            var merchant_key = _configuration.GetValue<string>("payFast:merchant_key");
            var passphrase = _configuration.GetValue<string>("payFast:passphrase");
            var api_version = "v1";

            // Create the request body
            var requestBody = new
            {
                merchant_id = merchant_id
            };

            //    var url = _configuration.GetValue<string>("payFast:endPoint");

            //  var requestUri = $"{url}/subscriptions/{token}/cancel?testing=true";

            //var jsonContent = JsonConvert.SerializeObject(requestBody);
            //var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            // Collect all parameters
            var parameters = new Dictionary<string, string>
            {
                { "merchant-id", merchant_id },
                { "timestamp", timestamp },
                { "version", "v1" } // API version
            };

            // Generate the signature
            var signature = GenerateSignature(parameters, passphrase);
            //var signature = GenerateSignature(merchant_id, timestamp, passphrase);

            /*var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = content
            };

            // Prepare the request message
            requestMessage.Headers.Add("merchant-id", merchant_id);
            requestMessage.Headers.Add("version", api_version);
            requestMessage.Headers.Add("timestamp", timestamp);
            requestMessage.Headers.Add("signature", signature);*/

            // Send the PUT request to cancel the subscription
            //var result = await _httpClient.SendAsync(requestMessage);

            // Send the PUT request to cancel the subscription
            //var response = await _httpClient.PutAsync(requestUri, content);

            //var subscriptionCancellation = new PayFastSubscription(this.payFastSettings);
            //var result = await subscriptionCancellation.Cancel(activeSubscription.subscription_token, istesting);

            // Cancel payfast subscription
            //if (result.status != "success")

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.payfast.co.za/subscriptions/{token}/cancel?testing=true");
            request.Headers.Add("merchant-id", merchant_id);
            request.Headers.Add("version", "v1");
            request.Headers.Add("timestamp", timestamp);
            request.Headers.Add("signature", signature);

            var response = await client.SendAsync(request);
            // response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            if (!response.IsSuccessStatusCode)
            {
                return UpdateResult.FailureResult("Did not cancel payfast subscription");
            }

            /*}

            _context.Subscriptions.Update(activeSubscription);
            Save();

            return UpdateResult.SuccessResultUpdate(activeSubscription.subscriptionId);*/

            // return UpdateResult.SuccessResultUpdate(activeSubscription.subscriptionId);
            return UpdateResult.SuccessResultUpdate(100);
        }

        public Task<UpdateResult> UpdateSubscription(Subscription subscription)
        {
            subscription.updated_at = DateTime.Now;
            subscription.isdeleted = false;

            _context.Subscriptions.Update(subscription);
            Save();

            return Task.FromResult(UpdateResult.SuccessResultUpdate(subscription.subscriptionId));
        }

        public Subscription GetSubscriptionById(int id)
        {
            return _context.Subscriptions
                    .Where(d => d.subscriptionId == id)
                    .First();
        }

        public Subscription GetActiveSubscriptionByUserProfileId(int id)
        {
            return _context.Subscriptions
                    .Where(d => d.userprofileid == id && d.isactive)
                    .First();
        }

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

        private async Task<NotifyResult> HandleSuccessfulPayment(PayFastNotify payFastNotify)
        {
            if (!int.TryParse(payFastNotify.custom_int1, out int userId) ||
               !int.TryParse(payFastNotify.custom_int2, out int packageId) ||
               !int.TryParse(payFastNotify.custom_int3, out int packagePrice)
               )
            {
                return NotifyResult.FailureResult("Invalid integer value in custom fields");
            }

            var activeSubscriptionResult = GetActiveSubscriptionByUserProfileId(userId);

            var newSubscription = new Subscription
            {
                subscriptionId = 0,
                userprofileid = userId,
                package_name = payFastNotify.custom_str1,
                package_id = packageId,
                package_price = packagePrice,
                start_date = DateTime.Now,
                end_date = DateTime.Now.AddMonths(12),
                subscription_duration = 365,
                subscription_token = payFastNotify.token,
                isactive = true
            };

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

            var cancelActiveSubscriptionResult = await CancelSubscription(activeSubscriptionResult.subscription_token);

            if (cancelActiveSubscriptionResult.Success)
            {
                var newSubscriptionResult = await CreateSubscription(newSubscription);

                if (newSubscriptionResult.Success)
                {
                    return NotifyResult.SuccessResult("Successfully updated subscription");
                }
                else
                {
                    return NotifyResult.FailureResult("Failed to update subscription");
                }
            }
            else
            {
                return NotifyResult.FailureResult("Failed to add subscription to database");
            }
        }

        private Task<NotifyResult> HandleFailedPayment()
        {
            return Task.FromResult(NotifyResult.FailureResult("Failed to add subscription"));
        }

        private Task<NotifyResult> HandlePendingPayment()
        {
            return Task.FromResult(NotifyResult.FailureResult("Pending adding of subscription"));
        }
        private string GenerateSignature(Dictionary<string, string> parameters)
        {
            // Sort the parameters alphabetically by key
            var sortedParameters = parameters
                .Where(p => !string.IsNullOrEmpty(p.Value)) // Exclude empty values
                .OrderBy(p => p.Key)
                .Select(p => $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value)}");

            // Concatenate sorted parameters
            var concatenated = string.Join("&", sortedParameters);

            // Generate the MD5 hash of the concatenated string
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public string GenerateSignature(Dictionary<string, string> data, string passphrase = "")
        {
            // Prepare the payload
            var payload = new StringBuilder();
            foreach (var key in data.Keys)
            {
                // Encode each key-value pair
                var encodedValue = HttpUtility.UrlEncode(data[key].Replace("+", " ")); // Matching the Python replace logic
                payload.Append($"{key}={encodedValue}&");
            }

            // Remove the trailing '&' character
            if (payload.Length > 0)
            {
                payload.Length--; // Remove last '&'
            }

            // Add passphrase if provided
            if (!string.IsNullOrEmpty(passphrase))
            {
                payload.Append($"&passphrase={passphrase}");
            }

            // Generate the MD5 hash
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(payload.ToString()));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
