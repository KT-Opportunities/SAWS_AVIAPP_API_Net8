using System.IdentityModel.Tokens.Jwt;

namespace SAWSCore8API.Models
{
    public class LoginExistResult : UpdateResult
    {
        public bool EmailExist { get; set; }

        public static LoginExistResult SuccessResult(bool exist)
        {
            return new LoginExistResult
            {
                Success = true,
                EmailExist = exist
            };
        }

        public static LoginExistResult FailureResult(string errorMessage)
        {
            return new LoginExistResult
            {
                Success = false,
                ErrorMessages = CreateError(errorMessage)
            };
        }
    }
}
