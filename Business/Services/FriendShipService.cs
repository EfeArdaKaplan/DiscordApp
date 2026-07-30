using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Abstract;
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
        private readonly IFriendNotifier _friendNotifier;

        public FriendShipService(ApiContext context, IMapper mapper, IFriendNotifier friendNotifier)
        {
            _context = context;
            _mapper = mapper;
            _friendNotifier = friendNotifier;
        }

        public async Task<bool> SendRequest(SendFriendRequestDto dto)
        {
            if (dto.SenderId == dto.ReceiverId) return false;

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

            await _friendNotifier.NotifyPendingCountChangedAsync(dto.ReceiverId); 

            return true;
        }

        public async Task<bool> RespondToRequest(FriendRequestActionDto dto, int respondingUserId)
        {
            var request = _context.FriendShips.FirstOrDefault(f => f.FriendShipId == dto.FriendShipId);
            if (request == null) return false;
            if (request.ReceiverId != respondingUserId) return false;
            if (request.Status != FriendShipStatus.Pending) return false;

            request.Status = dto.IsAccepted == 1 ? FriendShipStatus.Accepted : FriendShipStatus.Rejected;

            _context.SaveChanges();

            await _friendNotifier.NotifyPendingCountChangedAsync(respondingUserId); 

            return true;
        }

        public List<UserSummaryDto> GetFriends(int userId)
        {
            var friendIds = _context.FriendShips
                .Where(f => f.Status == FriendShipStatus.Accepted && (f.SenderId == userId || f.ReceiverId == userId))
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToList();

            return _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .ProjectTo<UserSummaryDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public List<PendingRequestDto> GetPendingRequests(int userId)
        {
            return _context.FriendShips
                .Where(f => f.ReceiverId == userId && f.Status == FriendShipStatus.Pending)
                .ProjectTo<PendingRequestDto>(_mapper.ConfigurationProvider)
                .ToList();
        }
    }
}