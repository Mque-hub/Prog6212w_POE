using Microsoft.AspNetCore.Http;

namespace RaceDay.Api.Models
{
    public class UploadBannerRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}