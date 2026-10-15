using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class UpdateEventRequest
    {
        public string EventName { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public DateOnly EventDate { get; set; }
        public TimeSpan EventDuration { get; set; }
        public string Location { get; set; }
        public string Distance { get; set; }
    }
}
