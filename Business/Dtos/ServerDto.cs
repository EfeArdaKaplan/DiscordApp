using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class ServerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ChannelDto> Channels { get; set; } = new();
    }
    public class ChannelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ServerId { get; set; }
    }

    public class MemberDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
    }
}
