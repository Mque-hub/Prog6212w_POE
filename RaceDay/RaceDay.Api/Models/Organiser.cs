namespace RaceDay.Api.Models
{
    public class Organiser
    {
        public int OrganiserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }

        // One Organiser -> many Events
        public ICollection<Event> Events { get; set; }
            = new List<Event>();
    }
}
