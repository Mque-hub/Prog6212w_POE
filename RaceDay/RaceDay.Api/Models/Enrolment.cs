namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        public int AccountID { get; set; }
        public Account Account { get; set; } = null!;

        public int EventID { get; set; }
        public Event Event { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public DateTime EnrolmentDate { get; set; }

        public RaceResults? RaceResult { get; set; }
    }
}
