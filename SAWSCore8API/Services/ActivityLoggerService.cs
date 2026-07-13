using SAWSCore8API.DbContexts;
using SAWSCore8API.Interfaces;
using SAWSCore8API.Models;

namespace SAWSCore8API.Services
{
    public class ActivityLoggerService : IActivityLoggerService
    {
        private readonly SAWSDbContext _context;

        public ActivityLoggerService(SAWSDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(HttpContext _context, ActivityLog activitylog)
        {
            activitylog.activityAction = _context.Request.Method;
            activitylog.path = _context.Request.Path;
            await SaveActivityAsync(activitylog);
        }


        private async Task<ActivityLog> SaveActivityAsync(ActivityLog activitylog)
        {
            try
            {
                _context.ActivityLogs.Add(activitylog);
                _context.SaveChanges();

                return activitylog;
            }
            catch (Exception err)
            {
                throw;
            }
        }
    }
}
