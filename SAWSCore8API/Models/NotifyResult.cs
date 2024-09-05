namespace SAWSCore8API.Models
{
    public class NotifyResult
    {
        public bool Success { get; set; }
        public string ErrorMessages { get; set; }
        public string SuccessMessages { get; set; }
        public static new NotifyResult SuccessResult(string message)
        {
            return new NotifyResult
            {
                Success = true,
                SuccessMessages = message
            };
        }

        public static new NotifyResult FailureResult(string message)
        {
            return new NotifyResult
            {
                Success = false,
                SuccessMessages = message
            };
        }
    }
}
