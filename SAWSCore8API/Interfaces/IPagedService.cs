using SAWSCore8API.Models;
using SAWSCore8API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IPagedService
    {
        Task<ResponseModel<List<UserProfileDto>>> GetPagedAllUsers([FromQuery] PaginationFilter filter, string role);

        Task<ResponseModel<List<Advert>>> GetPagedAllAdverts([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Feedback>>> GetPagedAllFeedbacks([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Feedback>>> GetPagedAllFeedbacksByUniqueEmail([FromQuery] PaginationFilter filter);

        Task<ResponseModel<List<Feedback>>> GetPagedAllBroadcasts([FromQuery] PaginationFilter filter);
  
    }
}