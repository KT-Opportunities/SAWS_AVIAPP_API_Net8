namespace SAWSCore8API.Models
{
    public class DeleteResult : UpdateResult
    {
        public string ErrorMessages { get; set; }
        public string SuccessMessages { get; set; }
        public static new DeleteResult SuccessResult(string message)
        {
            return new DeleteResult
            {
                Success = true,
                SuccessMessages = message
            };
        }

        public static new DeleteResult FailureResult(string message)
        {
            return new DeleteResult
            {
                Success = false,
                SuccessMessages = message
            };
        }
    }
}
