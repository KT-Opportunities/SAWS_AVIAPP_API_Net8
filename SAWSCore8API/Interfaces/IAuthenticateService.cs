using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IAuthenticateService
    {
        Task<LoginResult> LoginUser(Login appUser);
        Task<CreateResult> AddAdminUserProfile(RegisterAdmin appUser);
        Task<CreateResult> AddSubscriberUserProfile(RegisterSubscriber appUser);
        Task<UpdateResult> UpdateUserProfile(UserProfile user);
        Task<UpdateResult> UpdateIdentityEmail(UserProfile userProfile);

        // IEnumerable<Advert> GetAllAdverts();
        // Advert GetAdvertByAdvertId(int id);
        // void DeleteAdvertById(int id);
        void Save();
    }
}