using System.IdentityModel.Tokens.Jwt;

namespace SAWSCore8API.Models
{
    public class LoginResult : UpdateResult
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string AspUserId { get; set; }
        public string Fullname { get; set; }
        public string AspUserName { get; set; }
        public string AspUserEmail { get; set; }
        public List<string> RolesList { get; set; }
        public int UserProfileId { get; set; }
        public string UserProfileStatus { get; set; }

        public static LoginResult SuccessResult(JwtSecurityToken token, ApplicationUser user, UserProfile userProfile, List<string>rolesList)
        {
            return new LoginResult
            {
                Success = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                AspUserId = user.Id.ToString(),
                Fullname = userProfile?.fullname ?? string.Empty,
                AspUserName = user.UserName,
                AspUserEmail = user.Email,
                RolesList = rolesList,
                UserProfileId = userProfile?.userprofileid ?? 0,
                UserProfileStatus = userProfile != null ? "user profile exists" : "missing user profile"
            };
        }

        public static LoginResult FailureResult(string errorMessage)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessages = CreateError(errorMessage)
            };
        }
    }
}
