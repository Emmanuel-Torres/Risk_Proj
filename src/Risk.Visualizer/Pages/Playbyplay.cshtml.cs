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
            Status = ListOfMoves.Last();
            NumRows = Status.Board.Max(r => r.Location.Row);
            NumCols = Status.Board.Max(c => c.Location.Column);

        }
        private string GetActionFromCache()
        {
            return _cache.Get();
        }
        public IActionResult setActionToCache(string newAction)
        {
            _cache.Set(newAction);
            return RedirectToPage();
        }

        

    }
}
