using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Abstract;
using Core.Enums;
using Business.Dtos;
using Core.Entities;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    public class ServerService : IServerService
    {
        private readonly ApiContext _context;
        private readonly IMapper _mapper;
        private readonly IServerNotifier _serverNotifier;

        public ServerService(ApiContext context, IMapper mapper, IServerNotifier serverNotifier)
        {
            _context = context;
            _mapper = mapper;
            _serverNotifier = serverNotifier;
        }

        public async Task<List<ServerDto>> GetUserServersAsync(string userId)
        {
            // Gelen string ID'yi int'e (sayıya) çeviriyoruz
            if (!int.TryParse(userId, out int parsedUserId))
                return new List<ServerDto>();

            return await _context.UserServers
                .Where(us => us.UserId == parsedUserId)
                .Select(us => us.Server)
                .ProjectTo<ServerDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> CreateServerAsync(string serverName, string userId)
        {
            // Gelen string ID'yi int'e (sayıya) çeviriyoruz
            if (!int.TryParse(userId, out int parsedUserId))
                return false;

            var server = new Server
            {
                Name = serverName,
                OwnerId = parsedUserId
            };
            _context.Servers.Add(server);
            await _context.SaveChangesAsync();

            var defaultChannel = new Channel
            {
                Name = "genel",
                ServerId = server.Id
            };
            _context.Channels.Add(defaultChannel);

            var userServer = new UserServer
            {
                UserId = parsedUserId, 
                ServerId = server.Id
            };
            _context.UserServers.Add(userServer);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ChannelDto?> CreateChannelAsync(int serverId, string channelName, string userId)
        {
            if (!int.TryParse(userId, out int parsedUserId)) return null;
            if (string.IsNullOrWhiteSpace(channelName)) return null;

            bool isMember = await _context.UserServers
                .AnyAsync(us => us.ServerId == serverId && us.UserId == parsedUserId);
            if (!isMember) return null;

            var channel = new Channel { Name = channelName.Trim(), ServerId = serverId };
            _context.Channels.Add(channel);
            await _context.SaveChangesAsync();

            var channelDto =  _mapper.Map<ChannelDto>(channel); 

            var memberIds = await _context.UserServers
                .Where(us => us.ServerId == serverId)
                .Select(us => us.UserId)
                .ToListAsync();
            await _serverNotifier.NotifyChannelCreatedAsync(memberIds, channelDto);
            return channelDto;
        }

        public async Task<List<MemberDto>> GetServerMembersAsync(int serverId, string requestingUserId)
        {
            if (!int.TryParse(requestingUserId, out int parsedUserId))
                return new List<MemberDto>();

            // İsteği atan kullanıcı bu sunucunun üyesi değilse başka sunucunun üyelerini göremesin.
            bool isMember = await _context.UserServers
                .AnyAsync(us => us.ServerId == serverId && us.UserId == parsedUserId);

            if (!isMember)
                return new List<MemberDto>();

            return await _context.UserServers
                .Where(us => us.ServerId == serverId)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
        /*
        public async Task<bool> DeleteServerAsync(int serverId, string userId)
        {
            if (!int.TryParse(userId, out int parsedUserId))
                return false;

            var server = await _context.Servers
                .Include(s => s.Channels)
                .FirstOrDefaultAsync(s => s.Id == serverId);

            if (server == null || server.OwnerId != parsedUserId)
                return false;

            var userServers = await _context.UserServers
                .Where(us => us.ServerId == serverId)
                .ToListAsync();

            _context.UserServers.RemoveRange(userServers);
            _context.Channels.RemoveRange(server.Channels);
            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();
            return true; 
        }*/

        private async Task<int?> ParseUserIdAsync(string userId)
        {
            return int.TryParse(userId, out int id) ? id : null;
        }

        public async Task<bool> IsMemberAsync(int serverId, int userId)
        {
            return await _context.UserServers.AnyAsync(us => us.ServerId == serverId && us.UserId == userId);
        }

        public async Task<bool> IsChannelMemberAsync(int channelId, int userId)
        {
            var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
            if (channel == null) return false;

            return await IsMemberAsync(channel.ServerId, userId);
        }
        private async Task AddMemberIfNotExistsAsync(int serverId, int userId)
        {
            if (!await IsMemberAsync(serverId, userId))
                _context.UserServers.Add(new UserServer { UserId = userId, ServerId = serverId });
        }
        public async Task<string> GetInviteLinkAsync(int serverId, string userId)
        {
            var uid = await ParseUserIdAsync(userId);
            if (uid == null || !await IsMemberAsync(serverId, uid.Value)) return string.Empty;

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == serverId);
            if (server == null) return string.Empty;

            if (string.IsNullOrEmpty(server.InviteCode))
            {
                server.InviteCode = Guid.NewGuid().ToString("N")[..8];
                await _context.SaveChangesAsync();
            }
            return server.InviteCode;
        }

        public async Task<JoinByInviteResultDto> JoinByInviteCodeAsync(string inviteCode, string userId)
        {
            var uid = await ParseUserIdAsync(userId);
            if (uid == null) return new JoinByInviteResultDto { Success = false, Message = "Geçersiz kullanıcı." };

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.InviteCode == inviteCode);
            if (server == null) return new JoinByInviteResultDto { Success = false, Message = "Davet linki geçersiz." };

            bool wasAlreadyMember = await IsMemberAsync(server.Id, uid.Value);
            await AddMemberIfNotExistsAsync(server.Id, uid.Value);
            await _context.SaveChangesAsync();

            return new JoinByInviteResultDto { Success = true, ServerId = server.Id, Message = wasAlreadyMember ? "Zaten üyesin." : null };
        }

        public async Task<bool> SendServerInvitationAsync(SendServerInvitationDto dto, string senderUserId)
        {
            var sid = await ParseUserIdAsync(senderUserId);
            if (sid == null || !await IsMemberAsync(dto.ServerId, sid.Value)) return false;
            if (await IsMemberAsync(dto.ServerId, dto.ReceiverId)) return false;

            bool alreadyInvited = await _context.ServerInvitations.AnyAsync(si =>
                si.ServerId == dto.ServerId && si.ReceiverId == dto.ReceiverId && si.Status == FriendShipStatus.Pending);
            if (alreadyInvited) return false;

            _context.ServerInvitations.Add(new ServerInvitation { ServerId = dto.ServerId, SenderId = sid.Value, ReceiverId = dto.ReceiverId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RespondServerInvitationAsync(ServerInvitationActionDto dto, string receiverUserId)
        {
            var rid = await ParseUserIdAsync(receiverUserId);
            var invite = await _context.ServerInvitations.FirstOrDefaultAsync(si => si.Id == dto.InvitationId);
            if (rid == null || invite == null || invite.ReceiverId != rid.Value) return false;

            invite.Status = dto.IsAccepted == 1 ? FriendShipStatus.Accepted : FriendShipStatus.Rejected;
            if (invite.Status == FriendShipStatus.Accepted)
                await AddMemberIfNotExistsAsync(invite.ServerId, rid.Value);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PendingServerInvitationDto>> GetPendingServerInvitationsAsync(string userId)
        {
            var uid = await ParseUserIdAsync(userId);
            if (uid == null) return new();

            return await _context.ServerInvitations
                .Where(si => si.ReceiverId == uid.Value && si.Status == FriendShipStatus.Pending)
                .ProjectTo<PendingServerInvitationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}