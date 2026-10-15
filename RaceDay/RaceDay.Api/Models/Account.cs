namespace RaceDay.Api.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public int? ParticipantId { get; set; } // "?" implies that the id is nullable just as stated in the ERD

        public int? OrganiserId { get; set; }

        public Role Role { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
    }
}
