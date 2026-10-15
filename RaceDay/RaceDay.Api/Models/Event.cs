namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public DateOnly EventDate { get; set; }
        public TimeSpan EventDuration { get; set; }
        public string Location { get; set; }
        public string Distance { get; set; }
        public Organiser Organiser { get; set; } = null!;

        // One Event -> many Categories
        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        // One Event -> many Enrolments
        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();

    }
}
