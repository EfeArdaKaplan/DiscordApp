using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class User
    {
        public int UserId { get; set; } 
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public List<Message> Messages { get; set; } = new();
    }
}
