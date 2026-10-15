namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }
        public Participant Participant { get; set; } = null!;

        public int EventID { get; set; }
        public Event Event { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string EnrolmentType { get; set; }

        public RaceResults? RaceResult { get; set; }  // One Enrolment -> zero or one RaceResult
    }
}
