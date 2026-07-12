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
    }
}