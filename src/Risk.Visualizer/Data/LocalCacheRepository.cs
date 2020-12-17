using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Visualizer.Data
{
    public class LocalCacheRepository : ICacheService
    {
        public string action { get; set; }
        public string Get()
        {
            return action;
        }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(action);
        }

        public void Set(string action)
        {
            this.action = action;
        }
    }
}
