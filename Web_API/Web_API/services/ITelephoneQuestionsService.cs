using Web_API.Models;

namespace Web_API.services
{
    public interface ITelephoneQuestionsService
    {
        Task<ServiceResult> AddTelephoneInterviewQuestionsAsync(int postId, List<TelephonePostQuestionModel> questions);
        Task<List<TelephonePostQuestionModel>> GetTelephoneInterviewQuestionsByPostAsync(int postId);
        Task<ServiceResult> UpdateTelephoneInterviewQuestionAsync(int questionId, int postId, TelephonePostQuestionModel updatedQuestion);
        Task<ServiceResult> DeleteTelephoneInterviewQuestionAsync(int questionId, int postId);
    }
}
