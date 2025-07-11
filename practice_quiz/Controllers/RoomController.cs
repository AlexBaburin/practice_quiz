using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using practice_quiz.Models;

namespace practice_quiz.Controllers
{
    public class RoomController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Join()
        {
            return View();
        }

        public IActionResult Lobby(string roomCode, string playerName)
        {
            ViewBag.RoomCode = roomCode;
            ViewBag.PlayerName = playerName;
            return View();
        }

        public IActionResult StartQuiz(string roomCode, string playerName)
        {
            ViewBag.RoomCode = roomCode;
            ViewBag.PlayerName = playerName;
            return View();
        }

    }
}
