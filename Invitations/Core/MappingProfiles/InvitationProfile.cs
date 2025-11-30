using AutoMapper;
using Core.DTOs;
using Core.Models;
using System.Net.Mail;

namespace Core.MappingProfiles;

internal class InvitationProfile : Profile
{
    public InvitationProfile()
    {
        CreateMap<Invitation, InvitationsToReturnDto>()
            .ForMember(des => des.UserName, opt =>
            opt.MapFrom(src => new MailAddress(src.Email).User))

            .ForMember(des => des.ExpMessage, opt => opt
            .MapFrom(src => src.Status == Status.Pending ?
            $"Invitation expires in {src.CreatedAt.AddDays(7).Subtract(DateTime.Now).Days} days" : string.Empty))

            .ForMember(des => des.Status, opt => opt
            .MapFrom(src => src.Status.ToString()));
    }
}
