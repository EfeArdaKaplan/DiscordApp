using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ServerId { get; set; }
        public Server Server { get; set; } = null!;
    }
}
