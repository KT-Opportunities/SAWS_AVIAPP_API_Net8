namespace SAWSCore8API.Models
{
    public class UpdateResult
    {
        public bool Success { get; set; }
        public int OldId { get; set; }
        public IDictionary<string, IEnumerable<string>>? ErrorMessages { get; set; }

        public static UpdateResult SuccessResult()
        {
            return new UpdateResult { Success = true };
        }

        public static UpdateResult SuccessResultUpdate(int oldId)
        {
            return new UpdateResult
            {
                Success = true,
                OldId = oldId
            };
        }

        public static IDictionary<string, IEnumerable<string>> CreateError(string errorMessage)
        {
            var errors = new Dictionary<string, IEnumerable<string>>();
            errors[string.Empty] = new List<string>
            {
                errorMessage
            };

            return errors;
        }

        public static UpdateResult FailureResult(string errorMessage)
        {
            return new UpdateResult
            {
                Success = false,
                ErrorMessages = CreateError(errorMessage)
            };
        }
    }
}
