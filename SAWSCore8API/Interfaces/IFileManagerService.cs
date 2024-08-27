using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IFileManagerService
    {

        DocAdvert AddAdvertDoc(DocAdvert file);
        DocFeedback AddFeedbackDoc(DocFeedback file);
        DocAdvert UpdateAdvertDoc(DocAdvert file);
        DocFeedback UpdateFeedbackDoc(DocFeedback file);
        DocAdvert GetDocAdvertFileById(int Id);
        Task<DeleteResult> DeleteDocAdvertById(int id);

        // Task<CreateResult> AddAdvertDoc(DocAdvert file);
        // Task<UpdateResult> UpdateAdvertDoc(DocAdvert file);
        //Task<CreateResult> AddBroadcast(Feedback feedback, string batchId, string broadcastId);
        //Task<UpdateResult> UpdateBroadcast(Feedback feedback, string batchId, string broadcastId);
        //IEnumerable<FeedbackMessage> GetBroadcastMessages();
        //IEnumerable<Feedback> GetFeedbackMessagesBySenderId(string id);
        //Task<DeleteResult> DeleteBroadcastByBatchId(string id);
        void Save();
    }
}