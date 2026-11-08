using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class AuthResponse
    {
        public int UserID { get; set; }
        public string AuthenticationRole { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresInMinutes { get; set; }

    }
}
