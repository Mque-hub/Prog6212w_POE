using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class ResultDto
    {
        public int EnrolmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int FinishPosition { get; set; }
        public string PerformanceDetails { get; set; } = string.Empty;
        public List<string> Awards { get; set; } = new();

        public string EventName { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;

    }
}
