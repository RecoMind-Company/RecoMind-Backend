using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class InvitationService(IGrpcAuthenticationService grpcAuthenticationService,
                               IInvitationRepository repository,
                               IUnitOfWork unitOfWork,
                               IMapper mapper,
                               ILogger<InvitationService> logger) : IInvitationService
{

    public async Task<BaseToReturnDto> SendInvitationAsync(SendInvitationDto invitationDto)
    {
        var invitation = new Invitation
        {
            SenderId = invitationDto.SenderId,
            Email = invitationDto.Email,
            ReceiverRole = invitationDto.ReciverRole,
            CreatedAt = DateTime.UtcNow,
            CompanyId = invitationDto.CompanyId
        };

        var grpcResponse = await grpcAuthenticationService.Register(invitationDto.Email, invitationDto.ReciverRole);
        if (!grpcResponse.IsAuthenticated)
        {
            return new BaseToReturnDto
            {
                Message = $"Failed to send invitation: {grpcResponse.Message}"
            };
        }
        await repository.CreateAsync(invitation);
        await unitOfWork.Save();
        // Check if the gRPC authentication was successful

        return new BaseToReturnDto
        {
            IsSuccess = true,
            Message = "Invitation sent successfully.",
            UserId = grpcResponse.UserId
        };
    }
    public async Task<BaseToReturnDto> LoginAttemptWithInvitation(string email)
    {
        var invitation = await repository.Find(i => i.Email == email);
        if (invitation is null)
            return new BaseToReturnDto { IsSuccess = false, Message = "There is no invitation for this email" };
        if (invitation.Status == Status.Accepted)
            return new BaseToReturnDto { IsSuccess = true, Message = "Invitation is allready accepted." };
        var status = invitation.TryToAcceptInvitation();
        if (status == Status.Expired)
        {
            repository.Update(invitation);
            await unitOfWork.Save();
            return new BaseToReturnDto { IsSuccess = false, Message = "The invitation has expired." };
        }
        // the other case is status == Status.Accepted only so I removed the else if statement
        repository.Update(invitation);
        await unitOfWork.Save();
        return new BaseToReturnDto { IsSuccess = true, Message = "Invitation accepted successfully." };
    }

    public async Task<InvitationsToReturnDto> GetInvitationByIdAsync(int id)
    {
        var invitation = await repository.GetByIdAsync(id);
        if (invitation is null)
            return null;
        return mapper.Map<InvitationsToReturnDto>(invitation);
    }

    public async Task<IEnumerable<InvitationsToReturnDto>> GetInvitationsByStatus(GetInvitationDto getInvitationDto)
    {
        var invitations = await repository.FindAll(i => i.CompanyId == getInvitationDto.CompanyId
                                            && i.Status == Enum.Parse<Status>(getInvitationDto.Status));
        if (!invitations.Any())
            return Enumerable.Empty<InvitationsToReturnDto>();
        return mapper.Map<IEnumerable<InvitationsToReturnDto>>(invitations);
    }

    public async Task CheckAndExpireInvitations()
    {
        var expirationDate = DateTime.UtcNow.AddDays(-7);
        var pendingInvitations = await repository.FindAll(i => i.Status == Status.Pending && i.CreatedAt <= expirationDate);
        if (!pendingInvitations.Any())
            return;
        foreach (var invitation in pendingInvitations)
        {
            invitation!.Status = Status.Expired;
        }
        repository.UpdateRange(pendingInvitations!);
        await unitOfWork.Save();
        logger.LogInformation($"[HANGFIRE] Updated {pendingInvitations.Count()} invitations to Expired.");
    }
}