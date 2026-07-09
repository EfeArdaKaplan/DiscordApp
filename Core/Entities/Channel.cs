using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Channel
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public List<Message> Messages { get; set; } = new();
    }
}
