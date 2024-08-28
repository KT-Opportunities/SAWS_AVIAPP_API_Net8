using System.IdentityModel.Tokens.Jwt;

namespace SAWSCore8API.Models
{
    public class RequestPasswordResult
    {
        public bool Success { get; set; }
        // public string resetUrl { get; set; }
        // public string resetToken { get; set; }
        public string SuccessMessage{ get; set; }
        public string ErrorMessage { get; set; }

        public static RequestPasswordResult SuccessResult(string SuccessMessage)
        {
            return new RequestPasswordResult
            {
                Success = true,
                SuccessMessage = SuccessMessage
            };
        }

        public static RequestPasswordResult FailureResult(string errorMessage)
        {
            return new RequestPasswordResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
