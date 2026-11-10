using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RaceDay.Contracts.DTOs
{
    public class EventParticipantDto
    {
        public int EnrolmentID { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public int EventID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public DateTime EnrolmentDate { get; set; }
    }
}

