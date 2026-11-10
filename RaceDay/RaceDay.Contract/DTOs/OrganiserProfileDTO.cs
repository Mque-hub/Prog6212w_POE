
namespace RaceDay.Contracts.DTOs
{
    public class OrganiserProfileDTO
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }
    }
}
