using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Risk.Shared;

namespace Risk.Api.Controllers
{
    [ApiController]
    public class GameController : Controller
    {
        private IMemoryCache memoryCache;
        private readonly GameHolder gameHolder;
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;
        private readonly ILogger<GameRunner> logger;
        private readonly List<ApiPlayer> removedPlayers = new List<ApiPlayer>();
        //private bool mercenaries=false;

        public GameController(GameHolder gameHolder, IMemoryCache memoryCache, IHttpClientFactory client, IConfiguration config, ILogger<GameRunner> logger)
        {
            this.gameHolder = gameHolder;
            this.clientFactory = client;
            this.config = config;
            this.logger = logger;
            this.memoryCache = memoryCache;
            //this.mercenaries = mercenaries;
        }

        private async Task<bool> ClientIsRepsonsive(string baseAddress)
        {
            //client.CreateClient().BaseAddress = new Uri(baseAddress);
            var response = await clientFactory.CreateClient().GetStringAsync($"{baseAddress}/areYouThere");
            return response.ToLower() == "yes";
        }

        [HttpGet("status")]
        public IActionResult GameStatus()
        {
            GameStatus gameStatus;

            if (!memoryCache.TryGetValue("Status", out gameStatus))
            {
                gameStatus = gameHolder.game.GetGameStatus();

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
                memoryCache.Set("Status", gameStatus, cacheEntryOptions);
            }

            return Ok(gameStatus);
        }

        public static Game.Game InitializeGame (int height, int width, int numOfArmies, string gameMode)
        {
            GameStartOptions startOptions = new GameStartOptions {
                Height = height,
                Width = width,
                StartingArmiesPerPlayer = numOfArmies,
                GameMode = gameMode
            };

            Game.Game newGame = new Game.Game(startOptions);

            newGame.StartJoining();
            return newGame;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Join(JoinRequest joinRequest)
        {
            if (gameHolder.game.GameState == GameState.Joining && await ClientIsRepsonsive(joinRequest.CallbackBaseAddress))
            {
                var newPlayer = new ApiPlayer(
                    name: joinRequest.Name,
                    token: Guid.NewGuid().ToString(),
                    httpClient: clientFactory.CreateClient()
                );
                newPlayer.HttpClient.BaseAddress = new Uri(joinRequest.CallbackBaseAddress);

                gameHolder.game.AddPlayer(newPlayer);
                
                gameHolder.initialPlayers.Add(new ApiPlayer(
                    name: newPlayer.Name,
                    token: newPlayer.Token,
                    httpClient: newPlayer.HttpClient
                ));

                //this is where we add players to the new player list that is used to repopulate players when a game is restarted

                return Ok(new JoinResponse {
                    Token = newPlayer.Token
                });
            }
            else
            {
                return BadRequest("Unable to join game");
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> StartGame(StartGameRequest startGameRequest)
        {
            if(gameHolder.game.GameState != GameState.Joining)
            {
                return BadRequest("Game not in Joining state");
            }
            if(config["secretCode"] != startGameRequest.SecretCode)
            {
                return BadRequest("Secret code doesn't match, unable to start game.");
            }

            gameHolder.game.StartGame();
            var gameRunner = new GameRunner(gameHolder.game, logger);
            await gameRunner.StartGameAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult RestartGame(RestartGameRequest restartGameRequest)
        {
            if (restartGameRequest.RestartGame == true && restartGameRequest.GameState == GameState.GameOver)
            {
                var tempGame = InitializeGame(int.Parse(config["height"] ?? "5"),
                    int.Parse(config["width"] ?? "5"),
                    int.Parse(config["startingArmies"] ?? "5"),
                    config["gameMode"] ?? "Regular");
                
                foreach (var player in gameHolder.initialPlayers)
                {
                    tempGame.AddPlayer(player);
                }
                
                gameHolder.game = tempGame;

                return Ok();
            }

            return BadRequest("We cannot restart the game.");
        }
    }
}
