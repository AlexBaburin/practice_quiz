namespace practice_quiz.Models.Service
{
    public interface IQuizService
    {
        Task<List<Quiz>> GetAllQuizzesAsync();
        Task<Quiz?> GetQuizByIdAsync(int id);
        Task<Quiz> CreateQuizAsync(Quiz Quiz);
        Task<bool> UpdateQuizAsync(int id, Quiz Quiz);
        Task<bool> DeleteQuizAsync(int id);
    }
}
