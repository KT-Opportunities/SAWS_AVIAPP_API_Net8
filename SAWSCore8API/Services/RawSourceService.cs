using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

using PayFast;
using PayFast.AspNetCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Web;

namespace SAWSCore8API.Services
{
    public class RawSourceService : IRawSourceService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private ILogger<RawSourceService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public RawSourceService(
            SAWSDbContext context, 
            IUriService uriService, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<RawSourceService> logger, 
            IConfiguration configuration, 
            HttpClient httpClient
            )
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient ?? new HttpClient();
        }

        public Task<CreateResult> CreateSubscription(Subscription subscription)
        {
            subscription.created_at = DateTime.Now;
            subscription.updated_at = DateTime.Now;
            subscription.isdeleted = false;

            _context.Subscriptions.Add(subscription);
           

            return Task.FromResult(CreateResult.SuccessResult(subscription.subscriptionId));
        }

        
    }
}
