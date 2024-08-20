using System.IdentityModel.Tokens.Jwt;
using SAWSCore8API.Models;

namespace SAWSCore8API.Dto
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessages { get; set; }
        public static LoginResultDto FailureResult(string errorMessage)
        {
            return new LoginResultDto
            {
                Success = false,
                ErrorMessages = errorMessage
            };
        }
    }
}
