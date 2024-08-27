using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace SAWSCore8API.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ILogger<AuthenticateService> _logger;

        public FeedbackService(
                SAWSDbContext context,
                IUriService uriService,
                IHttpContextAccessor httpContextAccessor,
                IConfiguration configuration,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                ILogger<AuthenticateService> logger
            )
        {
            _context = context;
            _uriService = uriService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public Task<CreateResult> AddFeedback(Feedback feedback)
        {
            feedback.created_at = DateTime.Now;
            feedback.updated_at = DateTime.Now;
            feedback.isdeleted = false;

            _context.Feedbacks.Add(feedback);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(feedback.feedbackId));
        }

        public Task<CreateResult> AddBroadcast(Feedback feedback, string batchId, string broadcastId)
        {
            feedback.created_at = DateTime.Now;
            feedback.updated_at = DateTime.Now;
            feedback.batchId = batchId;
            feedback.isdeleted = false;

            _context.Feedbacks.Add(feedback);
            Save();

            return Task.FromResult(CreateResult.SuccessResult(feedback.feedbackId));
        }

        public Task<UpdateResult> UpdateFeedback(Feedback feedback)
        {
            feedback.updated_at = DateTime.Now;
            feedback.isdeleted = false;

            _context.Feedbacks.Update(feedback);
            Save();

            return Task.FromResult(UpdateResult.SuccessResultUpdate(feedback.feedbackId));
        }

        public Task<UpdateResult> UpdateBroadcast(Feedback feedback, string batchId, string broadcastId)
        {
            feedback.updated_at = DateTime.Now;
            feedback.isdeleted = false;

            _context.Feedbacks.Update(feedback);
            Save();

            return Task.FromResult(UpdateResult.SuccessResultUpdate(feedback.feedbackId));
        }

        public IEnumerable<FeedbackMessage> GetBroadcastMessages()
        {
            return _context.FeedbackMessages
                    .Where(d => d.isdeleted == false && d.broadcast != null)
                    .GroupBy(d => d.broadcastId)
                    .Select(group => group.First())
                    .ToList();
        }

        public IEnumerable<Feedback> GetFeedbackMessagesBySenderId(string id)
        {
            return _context.Feedbacks
                    .Where(fm => fm.senderId == id)
                    .OrderByDescending(d => d.feedbackId)
                    .ToList();
        }

        public Feedback GetFeedbackById(int id)
        {
            return _context.Feedbacks.Where(d => d.feedbackId == id)
                .Include(f => f.FeedbackMessages)
                .ThenInclude(fm => fm.DocFeedbacks)
                .First();
        }

        public Task<DeleteResult> DeleteFeedbackById(int id)
         {
            var feedback = _context.Feedbacks.First(a => a.feedbackId == id);    

            if (feedback != null)
            {
                feedback.isdeleted = true;
                feedback.deleted_at = DateTime.Now;

                Save();

                return Task.FromResult(DeleteResult.SuccessResult("Successfully deleted feedback"));
            }
            else
            {
                return Task.FromResult(DeleteResult.FailureResult("Failed to delete feedback"));
            }

        }

        public Task<DeleteResult> DeleteBroadcastByBatchId(string id)
        {
            var feedbacksToDelete = _context.Feedbacks.Where(a => a.batchId == id).ToList();

            if (feedbacksToDelete != null)
            {

                foreach (var feedback in feedbacksToDelete)
                {
                    feedback.isdeleted = true;
                    feedback.deleted_at = DateTime.Now;
                }

                Save();

                return Task.FromResult(DeleteResult.SuccessResult("Successfully deleted broadcast feedback"));
            }
            else
            {
                return Task.FromResult(DeleteResult.FailureResult("Failed to delete feedback"));
            }

        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
