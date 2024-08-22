using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;

namespace SAWSCore8API.Interfaces
{
    public interface IFileManagerService
    {

        // Task<CreateResult> AddAdvertDoc(DocAdvert file);
        DocAdvert AddAdvertDoc(DocAdvert file);
        // Task<UpdateResult> UpdateAdvertDoc(DocAdvert file);
        DocAdvert UpdateAdvertDoc(DocAdvert file);
        DocAdvert GetDocAdvertFileById(int Id);
        Task<DeleteResult> DeleteDocAdvertById(int id);

        //Task<CreateResult> AddBroadcast(Feedback feedback, string batchId, string broadcastId);
        //Task<UpdateResult> UpdateBroadcast(Feedback feedback, string batchId, string broadcastId);
        //IEnumerable<FeedbackMessage> GetBroadcastMessages();
        //IEnumerable<Feedback> GetFeedbackMessagesBySenderId(string id);
        //Task<DeleteResult> DeleteBroadcastByBatchId(string id);
        void Save();
    }
}