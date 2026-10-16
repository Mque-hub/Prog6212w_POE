namespace RaceDay.Api.Models
{
    public class RaceResults
    {
        public int RaceResultsID { get; set; }

        public int EnrolmentID { get; set; }
        public Enrolment Enrolment { get; set; } = null!;

        public TimeSpan FinishTime { get; set; }
        public int FinishPosition { get; set; }
        public string PerformanceDetails { get; set; }

        public ICollection<Awards> Awards { get; set; } = new List<Awards>();
    }
}
