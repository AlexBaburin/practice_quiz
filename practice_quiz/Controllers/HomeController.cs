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
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Leaderboards()
        {
            return View(await _context.Leaderboards.OrderByDescending(p => p.Score)
            .Take(10).ToListAsync());
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
