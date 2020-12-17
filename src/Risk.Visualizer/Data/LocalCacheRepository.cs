using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Visualizer.Data
{
    public class LocalCacheRepository : ICacheService
    {
        private int index = 0;
        public string action { get; set; }

        public void DecrementIndex()
        {
            if(index < 0)
            {
                index = 0;
            }
            else
            {
                index--;
            }
        }

        public string GetAction()
        {
            return action;
        }

        public int GetIndex()
        {
            return index;
        }

        public void IncrementIndex()
        {
            index++;
        }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(action);
        }

        public void MaxIndex(int max)
        {
            index = max;
        }

        public void ResetIndex()
        {
            index = 0;
        }

        public void SetAction(string action)
        {
            this.action = action;
        }

        
    }
}
