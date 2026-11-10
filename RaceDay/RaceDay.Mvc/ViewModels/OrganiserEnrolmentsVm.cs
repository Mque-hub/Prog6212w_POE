
using RaceDay.Contracts.DTOs;
using System.Collections.Generic;

namespace RaceDay.Mvc.ViewModels
{
    public class OrganiserEnrolmentsVm
    {
        
        public List<EventDto> Events { get; set; } = new();

       
        public List<EventParticipantDto> Participants { get; set; } = new();

       
        public int? SelectedEventId { get; set; }

        
        public string SelectedEventName { get; set; } = string.Empty;
    }
}
