using SAWSCore8API.Models;
using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly SAWSDbContext _context;
        private readonly IUriService _uriService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ILogger<FileManagerService> _logger;

        public FileManagerService(
                SAWSDbContext context,
                IUriService uriService,
                IHttpContextAccessor httpContextAccessor,
                IConfiguration configuration,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                ILogger<FileManagerService> logger
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

        public DocAdvert AddAdvertDoc(DocAdvert file)
        {

            try
            {
                var clpExist = _context.DocAdverts.FirstOrDefault(f => (f.advertId == file.advertId) && (f.DocTypeName == file.DocTypeName));

                if (clpExist == null)
                {
                    _context.DocAdverts.Add(file);
                    Save();
                }
                else
                {
                    _logger.LogWarning("Document advert exists");
                }

                return file;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with adding doc advert");
                throw;
            }

        }

        public DocAdvert UpdateAdvertDoc(DocAdvert file)
        {
            try
            {
                var clpExist = _context.DocAdverts.FirstOrDefault(f => (f.advertId == file.advertId) && (f.DocTypeName == file.DocTypeName));

                if (clpExist != null)
                {
                    file.isdeleted = false;

                    var local = _context.Set<DocAdvert>()
                    .Local
                    .FirstOrDefault(f => (f.advertId == file.advertId) && (f.DocTypeName == file.DocTypeName));

                    if (local != null)
                    {
                        _context.Entry(local).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }
                    _context.Entry(file).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    Save();

                }
                else
                {
                    _logger.LogWarning("Document advert with not found");
                }
                return file;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with adding doc advert");
                throw;
            }

        }


        // public IEnumerable<FeedbackMessage> GetBroadcastMessages()
        // {
        //     return _context.FeedbackMessages
        //             .Where(d => d.isdeleted == false && d.broadcast != null)
        //             .GroupBy(d => d.broadcastId)
        //             .Select(group => group.First())
        //             .ToList();
        // }

        // public IEnumerable<Feedback> GetFeedbackMessagesBySenderId(string id)
        // {
        //     return _context.Feedbacks
        //             .Where(fm => fm.senderId == id)
        //             .OrderByDescending(d => d.feedbackId)
        //             .ToList();
        // }

        public DocAdvert GetDocAdvertFileById(int Id)
        {
            return _context.DocAdverts.Where(d => d.Id == Id).FirstOrDefault();
        }

        public Task<DeleteResult> DeleteDocAdvertById(int id)
        {
            var doc = _context.DocAdverts.First(a => a.Id == id);

            if (doc != null)
            {
                doc.isdeleted = true;
                doc.deleted_at = DateTime.Now;

                Save();

                return Task.FromResult(DeleteResult.SuccessResult("Successfully deleted advert document"));
            }
            else
            {
                return Task.FromResult(DeleteResult.FailureResult("Failed to delete advert document"));
            }

        }

        // public Task<DeleteResult> DeleteBroadcastByBatchId(string id)
        // {
        //     var feedbacksToDelete = _context.Feedbacks.Where(a => a.batchId == id).ToList();

        //     if (feedbacksToDelete != null)
        //     {

        //         foreach (var feedback in feedbacksToDelete)
        //         {
        //             feedback.isdeleted = true;
        //             feedback.deleted_at = DateTime.Now;
        //         }

        //         Save();

        //         return Task.FromResult(DeleteResult.SuccessResult("Successfully deleted broadcast feedback"));
        //     }
        //     else
        //     {
        //         return Task.FromResult(DeleteResult.FailureResult("Failed to delete feedback"));
        //     }

        // }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
