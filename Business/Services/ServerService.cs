using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;

        public ServerService(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
    }
}