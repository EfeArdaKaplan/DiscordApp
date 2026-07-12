using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class UserServer
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int ServerId { get; set; }
        public Server Server { get; set; } = null!;
    }
}
