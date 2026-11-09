using RaceDay.Contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class ResultsVm
    {
        public List<ResultDto> Results { get; set; }
            = new List<ResultDto>();
    }
}
