using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Server
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OwnerId { get; set; } 

        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
        public ICollection<UserServer> UserServers { get; set; } = new List<UserServer>();
    }
}
