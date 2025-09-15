using System.Text.Json;
using AutoMapper;

using OdixPay.Notifications.Application.Services;
using OdixPay.Notifications.Domain.DTO.Requests;
using OdixPay.Notifications.Domain.Entities;
using OdixPay.Notifications.Domain.Interfaces;

namespace OdixPay.Notifications.Application.Mappings;

public class MappingProfile : Profile
{
    private readonly ITemplateEngine templateEngine = TemplateEngine.Instance;
    public MappingProfile()
    {
        // Add your AutoMapper mappings here
        // For example:
        CreateMap<Notification, NotificationResponse>().ForMember(dest => dest.Title, opt => opt.MapFrom(src => templateEngine.ProcessTemplate(src.Title, JsonSerializer.Deserialize<Dictionary<string, string>>(src.TemplateVariables ?? "{}", (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(), CancellationToken.None))).ForMember(dest => dest.Message, opt => opt.MapFrom(src => templateEngine.ProcessTemplate(src.Message, JsonSerializer.Deserialize<Dictionary<string, string>>(src.TemplateVariables ?? "{}", (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(), CancellationToken.None)));
        CreateMap<NotificationTemplate, TemplateResponse>();
        CreateMap<NotificationRecipient, NotificationRecipientResponseDTO>().ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Recipient));
    }
}
