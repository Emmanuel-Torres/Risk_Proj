using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Api
{
    public class GameHolder
    {
        public Game.Game game { get; set; }
        public ConcurrentBag<ApiPlayer> initialPlayers { get; set; }

        public GameHolder(Game.Game game, ConcurrentBag<ApiPlayer> initialPlayers)
        {
            this.game = game;
            this.initialPlayers = initialPlayers;
        }
    }
}
