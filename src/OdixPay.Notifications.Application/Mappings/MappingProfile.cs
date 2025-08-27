using AutoMapper;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;

namespace OdixPay.Notifications.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Add your AutoMapper mappings here
        // For example:
        CreateMap<Notification, NotificationResponse>();
        CreateMap<NotificationTemplate, TemplateResponse>();
        CreateMap<NotificationRecipient, NotificationRecipientResponseDTO>().ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Recipient));
    }
}
