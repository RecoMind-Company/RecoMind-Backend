using AutoMapper;
using Notification.Core.DTOs;
using Notification.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecoMind.Contracts.Events;


namespace Notification.Core.Mapper
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<NotificationModel, NotificationResponseDto>();
            CreateMap<NotificationEventDto, NotificationModel>();
        }
    }
}
