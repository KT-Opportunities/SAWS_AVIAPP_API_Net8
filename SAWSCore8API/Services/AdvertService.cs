using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SAWSCore8API.Controllers;

namespace SAWSCore8API.Services
{
    public class AdvertService : IAdvertService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private ILogger<AdvertService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdvertService(SAWSDbContext context, IUriService uriService, IHttpContextAccessor httpContextAccessor, ILogger<AdvertService> logger)
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Task<CreateResult> CreateAdvert(Advert advert)
        {
            advert.created_at = DateTime.Now;
            advert.updated_at = DateTime.Now;
            advert.isdeleted = false;

            _context.Adverts.Add(advert);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(advert.advertId));
        }

        public Task<CreateResult> AddAdvertClick(AdvertClick click)
        {
            click.created_at = DateTime.Now;
            click.updated_at = DateTime.Now;
            click.isdeleted = false;

            _context.AdvertClicks.Add(click);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(click.advertClickId));
        }

        public Task<UpdateResult> UpdateAdvert(Advert advert)
        {
            advert.updated_at = DateTime.Now;
            advert.isdeleted = false;

            _context.Adverts.Update(advert);
            Save();

            return Task.FromResult(UpdateResult.SuccessResultUpdate(advert.advertId));
        }

        public IEnumerable<Advert> GetAllAdverts()
        {
            return _context.Adverts
            .Where(d => d.isdeleted == false && d.ispublished == true)
            .Include(d => d.DocAdverts)
            .ToList();
        }

        public Advert GetAdvertByAdvertId(int id)
        {
            return _context.Adverts
                    .Where(d => d.advertId == id && d.isdeleted == false)
                    .Include(d => d.DocAdverts)
                    .First();
        }

        public Task<DeleteResult> DeleteAdvertById(int id)
        {
            var advert = _context.Adverts.First(a => a.advertId == id);

            if (advert != null)
            {
                advert.isdeleted = true;
                advert.deleted_at = DateTime.Now;

                Save();
                return Task.FromResult(DeleteResult.SuccessResult("Successfully deleted advert"));
            }
            else
            {
                return Task.FromResult(DeleteResult.FailureResult("Failed to delete user"));
            }
        }


        public void Save()
        {
            _context.SaveChanges();
        }


    }
}
