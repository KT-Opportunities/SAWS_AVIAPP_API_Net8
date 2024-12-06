using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IAuthenticateService
    {
        Task<LoginResult> LoginUser(LoginModel appUser);
        Task<CreateResult> AddAdminUserProfile(RegisterAdmin appUser);
        Task<CreateResult> AddSubscriberUserProfile(RegisterSubscriber appUser);
        Task<UpdateResult> UpdateUserProfile(UserProfile user);
        Task<UpdateResult> UpdateIdentityEmail(UserProfile userProfile);
        Task<DeleteResult> DeleteUserProfileById(int id);
        Task<LoggedInResult> GetLoggedInUser(string id);
        Task<LoginExistResult> LoginEmailExist(string email);
        Task<CreatePasswordResult> RequestPasswordReset(string email);
        Task<CreatePasswordResult> ResetPassword(IDResetPassword reset);
        Task<CreatePasswordResult> SendLogInCredentialsEmail(IDCredentials credentials);
        void Save();
    }
}