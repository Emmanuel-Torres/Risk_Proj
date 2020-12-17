using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Visualizer.Data
{
    public interface ICacheService
    {
        void SetAction(string action);
        
        string GetAction();
        
        void ResetIndex();
        
        void IncrementIndex();
        
        void DecrementIndex();
        
        void MaxIndex(int max);
        
        int GetIndex();
        
        bool IsEmpty();
    }
}
