
using RaceDay.Contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class OrganiserDashboardVm
    {
        // Organiser's events retrieved from the API.
        public List<EventDto> Events { get; set; }
            = new List<EventDto>();

        // Dashboard statistics.
        public int TotalEvents { get; set; }

        public int TotalEnrolments { get; set; }

        public int UpcomingEvents { get; set; }

        public int EventLocations { get; set; }
    }
}
