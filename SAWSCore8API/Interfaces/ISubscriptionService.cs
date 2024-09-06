using SAWSCore8API.Models;
using Microsoft.AspNetCore.Mvc;
using PayFast;

namespace SAWSCore8API.Interfaces
{
    public interface ISubscriptionService
    {
        Task<CreateResult> CreateSubscription(Subscription subscription);
        Task<CreateResult> CreateFreeSubscription(int userId);
        Task<CreateSubscriptionResult> RecuringPayment(Payment request);
        Task<CreateSubscriptionResult> OnceOffPayment(Payment request);
        Task<CreateSubscriptionResult> AdHocPayment(Payment request);
        Task<NotifyResult> NotifyITN(PayFastNotify payFastNotify);
        Task<CancelResult> CancelSubscription(string token);
        Task<UpdateResult> UpdateSubscription(Subscription subscription);
        Task<DeleteResult> DeleteSubscriptionById(int id);
        Subscription GetSubscriptionById(int id);
        Subscription GetActiveSubscriptionByUserProfileId(int id);
        void Save();
    }
}