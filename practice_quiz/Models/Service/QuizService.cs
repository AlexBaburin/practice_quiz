using Microsoft.EntityFrameworkCore;

namespace practice_quiz.Models.Service
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;

        public QuizService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Quiz>> GetAllQuizzesAsync() 
        { 
            var result = await _context.Quizzes.ToListAsync();
            return result;
        }
        public async Task<Quiz?> GetQuizByIdAsync(int id) 
        {
            return await _context.Quizzes.FindAsync(id);
        }
        public async Task<Quiz> CreateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }
        public async Task<bool> UpdateQuizAsync(int id, Quiz quiz)
        {
            if (id != quiz.QuizId)
                return false;

            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Quizzes.AnyAsync(c => c.QuizId == id))
                    return false;

                throw;
            }
        }
        public async Task<bool> DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
                return false;

            _context.Quizzes.Remove(quiz);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
