using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Visualizer.Data
{
    public interface ICacheService
    {
        void Set(string action);
        string Get();
        bool IsEmpty();
    
    }
}
