using AutoMapper;
using Business.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Server, ServerDto>();
            CreateMap<Channel, ChannelDto>();

            CreateMap<UserServer, MemberDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(stc => stc.User.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src => src.UserId == src.Server.OwnerId));
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderUserName, opt => opt.MapFrom(src => src.Sender.UserName));

            CreateMap<AppUser, UserSummaryDto>();

            CreateMap<FriendShip, PendingRequestDto>()
                .ForMember(dest => dest.SenderUserName, opt => opt.MapFrom(src => src.Sender.UserName));
        }
    }
}
