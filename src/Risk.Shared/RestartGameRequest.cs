using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class RestartGameRequest
    {
        public bool RestartGame { get; set; }
        public GameState GameState { get; set; }
    }
}
