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
        private readonly IServerService _serverService;

        public MessageService(ApiContext context, IMapper mapper, IMessageNotifier messageNotifier, IServerService serverService)
        {
            _context = context;
            _mapper = mapper;
            _messageNotifier = messageNotifier;
            _serverService = serverService;
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

            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

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

        public async Task<bool> SendChannelMessageAsync(int senderId, int channelId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;

            var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
            if (channel == null) return false;

            if (!await _serverService.IsMemberAsync(channel.ServerId, senderId)) return false;

            var message = new Message
            {
                SenderId = senderId,
                ChannelId = channelId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            var dto = _mapper.Map<MessageDto>(message);
            await _messageNotifier.NotifyNewChannelMessageAsync(channelId, dto);
            return true;
        }

        public async Task<List<MessageDto>?> GetChannelMessagesAsync(int channelId, int requestingUserId)
        {
            var channel = await _context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);
            if (channel == null) return null;

            if (!await _serverService.IsMemberAsync(channel.ServerId, requestingUserId)) return null;

            return await _context.Messages
                .Where(m => m.ChannelId == channelId)
                .OrderBy(m => m.SentAt)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
