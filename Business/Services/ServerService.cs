using Business.Abstract;
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

        public ServerService(ApiContext context)
        {
            _context = context;
        }

        public async Task<List<ServerDto>> GetUserServersAsync(string userId)
        {
            // Gelen string ID'yi int'e (sayıya) çeviriyoruz
            if (!int.TryParse(userId, out int parsedUserId))
                return new List<ServerDto>();

            return await _context.UserServers
                .Where(us => us.UserId == parsedUserId) 
                .Select(us => us.Server)
                .Select(s => new ServerDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Channels = s.Channels.Select(c => new ChannelDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ServerId = c.ServerId
                    }).ToList()
                })
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

        public async Task<bool> CreateChannelAsync(int serverId, string channelName, string userId)
        {
            if (!int.TryParse(userId, out int parsedUserId))
                return false;

            if (string.IsNullOrWhiteSpace(channelName))
                return false;

            // Kullanıcı gerçekten bu sunucunun üyesi mi? Değilse kanal ekleyemesin.
            bool isMember = await _context.UserServers
                .AnyAsync(us => us.ServerId == serverId && us.UserId == parsedUserId);

            if (!isMember)
                return false;

            var channel = new Channel
            {
                Name = channelName.Trim(),
                ServerId = serverId
            };

            _context.Channels.Add(channel);
            await _context.SaveChangesAsync();
            return true;
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

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == serverId);
            if (server == null)
                return new List<MemberDto>();

            return await _context.UserServers
                .Where(us => us.ServerId == serverId)
                .Select(us => new MemberDto
                {
                    Id = us.User.Id,
                    UserName = us.User.UserName ?? "Kullanıcı",
                    IsOwner = us.UserId == server.OwnerId
                })
                .ToListAsync();
        }
    }
}