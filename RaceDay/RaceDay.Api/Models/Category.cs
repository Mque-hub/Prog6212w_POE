namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        public Event Event { get; set; } = null!;

        // One Category -> many Enrolments
        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}
