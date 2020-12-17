using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Risk
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public GameStatus Status { get; set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }

        public async Task OnGet()
        {
            Status = await httpClientFactory.CreateClient().GetFromJsonAsync<GameStatus>($"{ configuration["ServerName"]}/status");
            NumRows = Status.Board.Max(r => r.Location.Row);
            NumCols = Status.Board.Max(c => c.Location.Column);
        }

        public async Task<IActionResult> OnPostStartGameAsync()
        {
            var client = httpClientFactory.CreateClient();
            Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["ServerName"]}/startgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );
            return new RedirectToPageResult("Index");
        }
        public async Task<IActionResult> OnPostRestartGameAsync()
        {
            Status = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["ServerName"]}/status");

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{configuration["ServerName"]}/restartgame", new RestartGameRequest { RestartGame = true, GameState = Status.GameState });
            if (response.IsSuccessStatusCode)
            {
                Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["ServerName"]}/startgame", new StartGameRequest { SecretCode = configuration["secretCode"] }));
            }
            else
            {
                throw new Exception("Cannot restart game");
            }

            return new RedirectToPageResult("Index");
        }
    }
}