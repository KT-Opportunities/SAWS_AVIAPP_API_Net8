namespace SAWSCore8API.Models
{
    public class CreateSubscriptionResult
    {
        public bool Success { get; set; }
        public string Url { get; set; }
        public string ErrorMessage { get; set; }


        public static CreateSubscriptionResult SuccessResult(string url)
        {
            return new CreateSubscriptionResult
            {
                Success = true,
                Url = url
            };
        }

        public static CreateSubscriptionResult FailureResult(string errorMessage)
        {
            return new CreateSubscriptionResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
