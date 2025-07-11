using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using practice_quiz.Models;
using practice_quiz.Models.Service;

namespace practice_quiz.Controllers
{
    public class QuizzesController : Controller
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService QuizService)
        {
            _quizService = QuizService;
        }

        public async Task<IActionResult> Questions()
        {
            return View(await _quizService.GetAllQuizzesAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizService.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuizId,Category,Description,Option1,Option2,Option3,Option4,Answer")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
               await _quizService.CreateQuizAsync(quiz);
               return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizService.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuizId,Category,Description,Option1,Option2,Option3,Option4,Answer")] Quiz quiz)
        {
            if (id != quiz.QuizId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _quizService.UpdateQuizAsync(id, quiz);
                }
                catch (Exception)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizService.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _quizService.DeleteQuizAsync(id);
            return RedirectToAction(nameof(Questions));
        }
    }
}
