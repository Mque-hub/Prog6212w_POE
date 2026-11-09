using RaceDay.Contracts.DTOs;
namespace RaceDay.Mvc.ViewModels
{
    public class MyEnrolmentsVm
    {
        public List<EnrolmentDto> Enrolments { get; set; }
            = new List<EnrolmentDto>();
    }
}
