using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IFeedbackService
    {

        Task<CreateResult> AddFeedback(Feedback feedback);
        Task<CreateResult> AddBroadcast(Feedback feedback, string batchId, string broadcastId);
        Task<UpdateResult> UpdateFeedback(Feedback feedback);
        Task<UpdateResult> UpdateBroadcast(Feedback feedback, string batchId, string broadcastId);
        IEnumerable<FeedbackMessage> GetBroadcastMessages();
        IEnumerable<Feedback> GetFeedbackMessagesBySenderId(string id);
        Feedback GetFeedbackById(int id);
        Task<DeleteResult> DeleteFeedbackById(int id);
        Task<DeleteResult> DeleteBroadcastByBatchId(string id);
        void Save();
    }
}