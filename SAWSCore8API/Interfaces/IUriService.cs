using SAWSCore8API.Models;

namespace SAWSCore8API.Interfaces
{
    public interface IUriService
    {
        public Uri GetPageUri(PaginationFilter filter, string route);
    }
}
