using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingIt.Maui.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

