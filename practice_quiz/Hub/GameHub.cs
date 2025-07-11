using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using practice_quiz.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace practice_quiz.Hub
{
    public interface IGameManager
    {
        Task StartGameAsync(string roomCode, Dictionary<string, List<Quiz>> questions,
            Dictionary<string, int> current);
        Task SubmitAnswerAsync(string roomCode, string playerName, int quizId, int selectedOption, int remainingTime, string connectionId);
    }

    public class GameManager : IGameManager
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly AppDbContext _context;
        public static Dictionary<string, List<Quiz>> RoomQuestions = new();
        public static Dictionary<string, int> RoomCurrentQuestion = new();
        private readonly IServiceScopeFactory _scopeFactory;

        public GameManager(IHubContext<GameHub> hubContext, AppDbContext context, IServiceScopeFactory scopeFactory)
        {
            _hubContext = hubContext;
            _context = context;
            _scopeFactory = scopeFactory;
        }

        public async Task SubmitAnswerAsync(string roomCode, string playerName, int quizId, int selectedOption, int remainingTime, string connectionId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return;
            bool isCorrect = selectedOption == quiz.Answer;

            if (isCorrect)
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Name == playerName && p.RoomCode == roomCode);

                if (player != null)
                {
                    player.Score += 1 + remainingTime;
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.Client(player.ConnectionId)
                        .SendAsync("UpdateScore", player.Score);
                }
            }

            _ = Task.Run(async () =>
            {
                await SendPlayerListToRoom(roomCode);
            });
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("AnswerResult", isCorrect);
        }

        public async Task StartGameAsync(string roomCode, Dictionary<string, List<Quiz>> questions, 
            Dictionary<string, int> current)
        {
            RoomQuestions = questions;
            RoomCurrentQuestion = current;
            await SendNextQuestion(roomCode);
        }

        private async Task SendNextQuestion(string roomCode)
        {
            if (!RoomQuestions.ContainsKey(roomCode)) return;

            await Task.Delay(500);
            int currentIndex = RoomCurrentQuestion[roomCode];
            var questions = RoomQuestions[roomCode];

            if (currentIndex >= questions.Count)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var players = await context.Players
                    .Where(p => p.RoomCode == roomCode)
                    .Select(p => new PlayerViewModel
                    {
                        PlayerId = p.PlayerId,
                        Name = p.Name,
                        Score = p.Score,
                        RoomCode = p.RoomCode
                    }).ToListAsync();
                foreach (var player in players)
                {
                    var entry = new Leaderboard
                    {
                        Name = player.Name,
                        Score = player.Score
                    };
                    if (!context.Leaderboards.Any(l => l.Name == player.Name && l.Score == player.Score))
                    {
                        context.Leaderboards.Add(entry);
                    }
                }
                await context.SaveChangesAsync();

                await _hubContext.Clients.Group(roomCode).SendAsync("GameEnded", players);

                return;
            }

            var question = questions[currentIndex];
            RoomCurrentQuestion[roomCode] = currentIndex + 1;

            await _hubContext.Clients.Group(roomCode).SendAsync("ReceiveQuestion", new
            {
                question.QuizId,
                question.Description,
                question.Option1,
                question.Option2,
                question.Option3,
                question.Option4
            });

            await Task.Delay(500); // small UI delay

            _ = Task.Run(async () => 
            { 
                await SendPlayerListToRoom(roomCode); 
            });
            _ = Task.Run(async () =>
            {
                await RunTimer(roomCode, question.QuizId, 10);
            });
        }

        private async Task RunTimer(string roomCode, int quizId, int seconds)
        {
            for (int timeLeft = seconds; timeLeft >= 0; timeLeft--)
            {
                await _hubContext.Clients.Group(roomCode).SendAsync("UpdateTimer", timeLeft);
                await Task.Delay(1000);
            }

            await _hubContext.Clients.Group(roomCode).SendAsync("TimeUp", quizId);
            await Task.Delay(2000);
            await SendNextQuestion(roomCode);
        }

        public async Task SendPlayerListToRoom(string roomCode)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var players = await context.Players
                .Where(p => p.RoomCode == roomCode)
                .Select(p => new PlayerViewModel
                {
                    PlayerId = p.PlayerId,
                    Name = p.Name,
                    Score = p.Score,
                    RoomCode = p.RoomCode
                }).ToListAsync();

            await _hubContext.Clients.Group(roomCode).SendAsync("PlayerListUpdated", players);
        }

    }

    public class GameHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AppDbContext _context;

        private static Dictionary<string, List<Quiz>> RoomQuestions = new();
        private static Dictionary<string, int> RoomCurrentQuestion = new();
        private readonly IHubContext<GameHub> _hubContext;
        private readonly IGameManager _gameManager;

        public GameHub(AppDbContext context, IHubContext<GameHub> hubContext, IGameManager gameManager)
        {
            _context = context;
            _hubContext = hubContext;
            _gameManager = gameManager;
        }

        public async Task JoinGroup(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        }

        public async Task SubmitAnswer(string roomCode, string playerName, int quizId, int selectedOption, int remainingTime)
        {
            var player = _context.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) {
                _context.Players.Add(new Player {
                    ConnectionId = Context.ConnectionId,
                    RoomCode = roomCode,
                    Name = playerName,
                    Score = 0,
                });

                await _context.SaveChangesAsync();
            }

            await _gameManager.SubmitAnswerAsync(roomCode, playerName, quizId, selectedOption, remainingTime, Context.ConnectionId);
        }


        public async Task StartGame(string roomCode)
        {
            var questions = _context.Quizzes.OrderBy(q => Guid.NewGuid()).Take(10).ToList();

            RoomQuestions[roomCode] = questions;
            RoomCurrentQuestion[roomCode] = 0;

            await Clients.Group(roomCode).SendAsync("GameStarted");
            await _gameManager.StartGameAsync(roomCode, RoomQuestions, RoomCurrentQuestion);
        }

        public async Task CreateRoom(string playerName)
        {
            var roomCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
            var connectionId = Context.ConnectionId;

            var room = new Room
            {
                RoomCode = roomCode,
                HostConnectionId = connectionId,
                GameStarted = false
            };

            var player = new Player
            {
                Name = playerName,
                Score = 0,
                ConnectionId = connectionId,
                RoomCode = roomCode
            };

            room.Players.Add(player);
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            await Groups.AddToGroupAsync(connectionId, roomCode);

            await Clients.Caller.SendAsync("RoomCreated", roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName);

            await SendPlayerListToRoom(roomCode);
        }

        public async Task JoinRoom(string roomCode, string playerName)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomCode == roomCode);

            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found.");
                return;
            }

            var connectionId = Context.ConnectionId;

            var player = new Player
            {
                Name = playerName,
                Score = 0,
                ConnectionId = connectionId,
                RoomCode = roomCode
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            await Groups.AddToGroupAsync(connectionId, roomCode);

            await Clients.Caller.SendAsync("RoomJoined", roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName);
            await SendPlayerListToRoom(roomCode);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = _context.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

            if (player != null)
            {
                var roomCode = player.RoomCode;
                var name = player.Name;

                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

                await SendPlayerListToRoom(roomCode);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task GetPlayersInRoom(string roomCode)
        {
            var players = _context.Players
                .Where(p => p.RoomCode == roomCode)
                .Select(p => p.Name)
                .ToList();

            await Clients.Caller.SendAsync("ReceivePlayerList", players);
        }

        private async Task SendPlayerListToRoom(string roomCode)
        {
            var players = _context.Players
                .Where(p => p.RoomCode == roomCode)
                .Select(p => p.Name)
                .ToList();

            await Clients.Group(roomCode).SendAsync("ReceivePlayerList", players);
        }
    }
}
