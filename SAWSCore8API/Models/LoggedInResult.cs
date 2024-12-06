using System.IdentityModel.Tokens.Jwt;

namespace SAWSCore8API.Models
{
    public class LoggedInResult : UpdateResult
    {
        public string userID { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string userRole { get; set; }
        // public string erromessage { get; set; }


        public static LoggedInResult SuccessResult(ApplicationUser user, string rolesList)
        {
            return new LoggedInResult
            {
                Success = true,
                userID = user.Id,
                userEmail = user.Email,
                userName = user.UserName,
                firstname = user.FirstName,
                lastname = user.LastName,
                userRole = rolesList
            };
        }

        public static LoggedInResult FailureResult(string errorMessage)
        {
            return new LoggedInResult
            {
                Success = false,
                ErrorMessages = CreateError(errorMessage)
            };
        }
    }
}
