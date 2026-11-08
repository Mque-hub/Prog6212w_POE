namespace RaceDay.Mvc.ViewModels
{
    public class EventParticipantViewModel
    {
        public string ParticipantName { get; set; }
        public string CategoryName { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string? Status { get; set; }
    }
}
