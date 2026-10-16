using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class ResultRequest
    {
        public TimeSpan FinishTime { get; set; }
        public int FinishPosition { get; set; }
        public string PerformanceDetails { get; set; } = string.Empty;
        public List<string> Awards { get; set; } = new();
    }
}
