using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Controllers
{
    /// <summary>
    /// Minimal return/cancel landing pages for PayFast redirects.
    /// PayFast requires absolute HTTPS return/cancel URLs.
    /// </summary>
    [ApiController]
    [Route("v1/subscriptions/payfast")]
    public class PayFastRedirectController : ControllerBase
    {
        private const string Html = "<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"/><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"/><title>Payment</title></head><body style=\"font-family:Arial,Helvetica,sans-serif;padding:24px;\"><h3>You can return to the app.</h3><p>You may close this window and return to the application.</p><script>(function(){try{window.close();}catch(e){} setTimeout(function(){try{window.close();}catch(e){}}, 250);})();</script><noscript>You can return to the app.</noscript></body></html>";

        /// <summary>
        /// PayFast return URL endpoint.
        /// Accepts arbitrary query parameters supplied by PayFast.
        /// </summary>
        [HttpGet("return")]
        [Produces("text/html")]
        public ContentResult Return()
        {
            return Content(Html, "text/html");
        }

        /// <summary>
        /// PayFast cancel URL endpoint.
        /// Accepts arbitrary query parameters supplied by PayFast.
        /// </summary>
        [HttpGet("cancel")]
        [Produces("text/html")]
        public ContentResult Cancel()
        {
            return Content(Html, "text/html");
        }
    }
}
