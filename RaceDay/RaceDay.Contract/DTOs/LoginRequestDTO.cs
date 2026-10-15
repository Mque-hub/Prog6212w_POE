using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts.DTOs
{
    public class LoginRequestDTO
    {
        public string AuthenticationRole { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
