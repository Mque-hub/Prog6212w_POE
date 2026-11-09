using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int AccountId { get; set; }
        public int EventID { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; }

        // Additional information for displaying enrolments
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
