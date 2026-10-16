using Microsoft.Identity.Client;

namespace RaceDay.Api.Models
{
    public class Awards
    {
        public int AwardsId { get; set; }
        public string AwardsDetails { get; set; }
        public int RaceResultsID { get; set; }
        public RaceResults RaceResults { get; set; } = null!;
    }
}
