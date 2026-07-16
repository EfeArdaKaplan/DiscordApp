using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Dtos;
using Core.Entities;
using Core.Enums;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Services
{
    public class FriendShipService
    {
        private readonly ApiContext _context;
        private readonly IMapper _mapper;
        public FriendShipService(ApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public bool SendRequest(SendFriendRequestDto dto)
        {
            // kendine istek atamama olayı
            if (dto.SenderId == dto.ReceiverId) return false;

            // zaten var olan bir istek veya arkaşlık var mı 
            var existing = _context.FriendShips.FirstOrDefault(f =>
                (f.SenderId == dto.SenderId && f.ReceiverId == dto.ReceiverId) ||
                (f.SenderId == dto.ReceiverId && f.ReceiverId == dto.SenderId));

            if (existing != null) return false;

            var friendship = new FriendShip
            {
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                Status = FriendShipStatus.Pending,
                CreateAt = DateTime.UtcNow
            };

            _context.FriendShips.Add(friendship);
            _context.SaveChanges();
            return true;
        }

        // bekleyen isteklers
        public bool RespondToRequest(FriendRequestActionDto dto)
        {
            var request = _context.FriendShips.FirstOrDefault(f => f.FriendShipId == dto.FriendShipId);
            if (request == null) return false;

            if (dto.IsAccepted == 1)
            {
                request.Status = FriendShipStatus.Accepted;
            }
            else
            {
                request.Status = FriendShipStatus.Rejected;
            }

            _context.SaveChanges();
            return true;
        }

        // bir kullanıcının Arkadaş listesini getirme
        public List<AppUser> GetFriends(int userId)
        {
            var friendIds = _context.FriendShips
                .Where(f => f.Status == FriendShipStatus.Accepted && (f.SenderId == userId || f.ReceiverId == userId))
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToList();
            return _context.Users.Where(u => friendIds.Contains(u.Id)).ToList();
        }

        public List<PendingRequestDto> GetPendingRequests (int userId)
        {
            return _context.FriendShips
                .Where(f => f.ReceiverId == userId && f.Status == FriendShipStatus.Pending)
                .ProjectTo<PendingRequestDto>(_mapper.ConfigurationProvider)
                .ToList();
        }
    }
}
