using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Dtos
{
    public class SendServerInvitationDto
    {
        public int ServerId { get; set; }
        public int ReceiverId { get; set; }
    }

    public class ServerInvitationActionDto
    {
        public int InvitationId { get; set; }
        public int IsAccepted { get; set; }
    }

    public class PendingServerInvitationDto
    {
        public int InvitationId { get; set; }
        public int ServerId { get; set; }
        public string ServerName { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public string SenderUserName { get; set; } = string.Empty;
    }

    public class JoinByInviteResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? ServerId { get; set; }
    }

    public class JoinByInviteDto
    {
        public string InviteCode { get; set; } = string.Empty;
    }

}
