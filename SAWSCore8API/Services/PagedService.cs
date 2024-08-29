using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAWSCore8API.Dtos;

namespace SAWSCore8API.Services
{
    public class PagedService : IPagedService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public PagedService(SAWSDbContext context, IUriService uriService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel<List<UserProfileDto>>> GetPagedAllUsers([FromQuery] PaginationFilter filter, string role)
        {
            var route = _httpContextAccessor.HttpContext?.Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            var pagedData = _context.userProfiles
            .Where(d => d.isdeleted == false && d.userrole == role)
            .Include(d => d.Subscription)
            .OrderByDescending(d => d.userprofileid)
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .Select(d => new UserProfileDto
            {
                userprofileid = d.userprofileid,
                fullname = d.fullname,
                email = d.email,
                username = d.username,
                userrole = d.userrole,
                aspuid = d.aspuid,
                isactive = d.isactive,
                created_at = d.created_at,
                Subscription = d.Subscription
                                .Where(s => s.isactive)
                                .Select(s => new SubscriptionDto
                                {
                                    subscriptionId = s.subscriptionId,
                                    package_name = s.package_name,
                                    isactive = s.isactive,
                                })
                                .ToList()
            })
            .ToList();

            var totalRecords = _context.userProfiles.Where(d => d.isdeleted == false && d.userrole == role).Count();

            var pagedReponse = PaginationConfig.CreatePagedReponse<UserProfileDto>(pagedData, validFilter, totalRecords, _uriService, route);

            return pagedReponse;

        }

        public async Task<ResponseModel<List<Advert>>> GetPagedAllAdverts([FromQuery] PaginationFilter filter)
        {
            var route = _httpContextAccessor.HttpContext?.Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            var pagedData = _context.Adverts
                .Where(d => d.isdeleted == false)
                .OrderByDescending(d => d.advertId)
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            var totalRecords = _context.Adverts.Where(d => d.isdeleted == false).Count();

            var pagedReponse = PaginationConfig.CreatePagedReponse<Advert>(pagedData, validFilter, totalRecords, _uriService, route);
            return pagedReponse;

        }

        public async Task<ResponseModel<List<Feedback>>> GetPagedAllFeedbacks([FromQuery] PaginationFilter filter)
        {
            var route = _httpContextAccessor.HttpContext?.Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            var pagedData = _context.Feedbacks
                .Where(d => d.isdeleted == false && d.broadcasterId == null)
                .OrderByDescending(d => d.feedbackId)
               .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
               .Take(validFilter.PageSize)
               .ToList();

            var totalRecords = _context.Feedbacks.Where(d => d.isdeleted == false && d.broadcasterId == null).Count();

            var pagedReponse = PaginationConfig.CreatePagedReponse<Feedback>(pagedData, validFilter, totalRecords, _uriService, route);
            return pagedReponse;

        }

        public async Task<ResponseModel<List<Feedback>>> GetPagedAllFeedbacksByUniqueEmail([FromQuery] PaginationFilter filter)
        {
            List<Feedback> toReturn;

            var allFeedbacks = _context.Feedbacks
                .Where(d => d.isdeleted == false)
                .OrderByDescending(d => d.feedbackId)
                .ToList();

            var route = _httpContextAccessor.HttpContext?.Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            toReturn = allFeedbacks
                .GroupBy(d => d.senderEmail)
                .Select(group => group.First())
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            var pagedData = toReturn;

            var totalRecords = allFeedbacks
                                .GroupBy(d => d.senderEmail)
                                .Count();

            var pagedReponse = PaginationConfig.CreatePagedReponse<Feedback>(pagedData, validFilter, totalRecords, _uriService, route);
            return pagedReponse;

        }

        public async Task<ResponseModel<List<Feedback>>> GetPagedAllBroadcasts([FromQuery] PaginationFilter filter)
        {

            List<Feedback> toReturn;

            var allFeedbacks = _context.Feedbacks
                .Where(d => d.isdeleted == false && d.broadcasterId != null)
                .OrderByDescending(d => d.feedbackId)
                .ToList();

            var route = _httpContextAccessor.HttpContext?.Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            toReturn = allFeedbacks
                .GroupBy(d => d.batchId)
                .Select(group => group.First())
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToList();

            var pagedData = toReturn;

            var totalRecords = allFeedbacks
                                .GroupBy(d => d.batchId)
                                .Count();

            var pagedReponse = PaginationConfig.CreatePagedReponse<Feedback>(pagedData, validFilter, totalRecords, _uriService, route);
            return pagedReponse;

        }

    }
}
