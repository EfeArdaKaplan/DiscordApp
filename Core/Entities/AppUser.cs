using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class AppUser:IdentityUser<int>
    {
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public List<Message> Messages { get; set; } = new();
    }
}
