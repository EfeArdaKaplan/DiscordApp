using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class RegisterDto
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
