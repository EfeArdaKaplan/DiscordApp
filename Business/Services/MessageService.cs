using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Abstract;
using Business.Dtos;
using Core.Entities;
using Core.Enums;
using Data.Context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApiContext _context;
        private readonly IMapper _mapper;
        private readonly IMessageNotifier _messageNotifier;

        public MessageService(ApiContext context, IMapper mapper, IMessageNotifier messageNotifier)
        {
            _context = context;
            _mapper = mapper;
            _messageNotifier = messageNotifier;
        }

        public async Task<bool> SendDirectMessageAsync(int senderId, int receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || senderId == receiverId)
                return false;

            bool areFriends = await _context.FriendShips.AnyAsync(f =>
                f.Status == FriendShipStatus.Accepted &&
                ((f.SenderId == senderId && f.ReceiverId == receiverId) ||
                 (f.SenderId == receiverId && f.ReceiverId == senderId)));

            if(!areFriends) return false;

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            
            var dto = _mapper.Map<MessageDto>(message);
            await _messageNotifier.NotifyNewMessageAsync(receiverId, dto);
            return true;
        }
        public async Task<List<MessageDto>> GetConversationAsync(int userId, int otherUserId)
        {
            return await _context.Messages
                .Where(m => m.ChannelId == null &&
                    ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == userId)))
                .OrderBy(m => m.SentAt)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
