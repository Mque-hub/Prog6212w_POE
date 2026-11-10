using RaceDay.Contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantDashboardVm
    {
      
        public int TotalUpcomingEvents { get; set; }

        public int TotalEnrolments { get; set; }

        public int CompletedResults { get; set; }

        public int? BestFinishPosition { get; set; }

        
        public List<EventDto> UpcomingEvents { get; set; } = new();

        
        public List<ResultDto> RecentResults { get; set; } = new();
    }
}
