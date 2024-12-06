namespace SAWSCore8API.Models
{
    public class CancelResult : UpdateResult
    {
        public string Message { get; set; }

        public static CancelResult SuccessResult(string message)
        {
            return new CancelResult
            {
                Success = true,
                Message = message,
            };
        }

        public static CancelResult FailureResult(string errorMessage)
        {
            return new CancelResult
            {
                Success = false,
                ErrorMessages = CreateError(errorMessage)
            };
        }
    }
}
