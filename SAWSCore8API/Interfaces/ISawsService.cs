using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface ISawsService
    {
        Task<ResponseModel<List<UserProfile>>> GetPagedAllAdmins([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Advert>>> GetPagedAllAdverts([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Feedback>>> GetPagedAllFeedbacks([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Feedback>>> GetPagedAllFeedbacksByUniqueEmail([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Feedback>>> GetPagedAllBroadcasts([FromQuery] PaginationFilter filter);
        
        Task<ResponseModel<List<UserProfile>>> GetPagedAllSubscribers([FromQuery] PaginationFilter filter);
    }
}