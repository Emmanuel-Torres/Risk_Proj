using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Risk.Shared;
using Risk.Visualizer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Risk.Visualizer.Pages
{
    public class PlayByPlayModel : PageModel
    {

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly ICacheService _cache;

        public PlayByPlayModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ICacheService cache)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            _cache = cache;
        }

        public GameStatus Status { get; set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public IEnumerable<GameStatus> ListOfMoves { get; private set; }

        public async Task OnGet()
        {
            ListOfMoves = await httpClientFactory.CreateClient().GetFromJsonAsync<IEnumerable<GameStatus>>($"{ configuration["ServerName"]}/playbyplay");
            Status = GetActionFromCache(ListOfMoves, _cache.GetIndex());
            NumRows = Status.Board.Max(r => r.Location.Row);
            NumCols = Status.Board.Max(c => c.Location.Column);
        }

        private GameStatus GetActionFromCache(IEnumerable<GameStatus> moves, int currentIndex)
        {
            if (_cache.GetAction() == "first")
            {
                _cache.ResetIndex();
                return moves.First();
            }
            else if (_cache.GetAction() == "nextMove")
            {
                if (currentIndex == moves.Count() - 1)
                {
                    return moves.ElementAt(_cache.GetIndex());
                }
                else
                {
                    _cache.IncrementIndex();
                    return moves.ElementAt(_cache.GetIndex());
                }
            }
            else if (_cache.GetAction() == "previousMove")
            {
                if (currentIndex < 1)
                {
                    return moves.ElementAt(_cache.GetIndex());
                }
                else {
                    _cache.DecrementIndex();
                    return moves.ElementAt(_cache.GetIndex());
                }
            }
            else
            {
                _cache.MaxIndex(moves.Count() - 1);
                return moves.Last();
            }
        }

        public IActionResult OnPostSetActionToCache(string action)
        {
            _cache.SetAction(action);
            return RedirectToPage();
        }
    }
}
