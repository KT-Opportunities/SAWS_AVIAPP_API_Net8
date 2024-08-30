using System.IdentityModel.Tokens.Jwt;

namespace SAWSCore8API.Models
{
    public class CreatePasswordResult
    {
        public bool Success { get; set; }
        // public string resetUrl { get; set; }
        // public string resetToken { get; set; }
        public string SuccessMessage{ get; set; }
        public string ErrorMessage { get; set; }

        public static CreatePasswordResult SuccessResult(string SuccessMessage)
        {
            return new CreatePasswordResult
            {
                Success = true,
                SuccessMessage = SuccessMessage
            };
        }

        public static CreatePasswordResult FailureResult(string errorMessage)
        {
            return new CreatePasswordResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
