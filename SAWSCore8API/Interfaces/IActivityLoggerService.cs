using SAWSCore8API.Models;

namespace SAWSCore8API.Interfaces
{
    public interface IActivityLoggerService
    {
        Task LogAsync(HttpContext context, ActivityLog activitylog);
    }
}
