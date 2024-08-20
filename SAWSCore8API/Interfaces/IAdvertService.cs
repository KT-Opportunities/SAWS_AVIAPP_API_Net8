using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IAdvertService
    {
        Task<CreateResult> CreateAdvert(Advert advert);
        Task<UpdateResult> UpdateAdvert(Advert advert);
        Task<CreateResult> AddAdvertClick(AdvertClick click);
        IEnumerable<Advert> GetAllAdverts();
        Advert GetAdvertByAdvertId(int id);
        Task<DeleteResult> DeleteAdvertById(int id);
        void Save();
    }
}