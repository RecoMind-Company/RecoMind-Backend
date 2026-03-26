using Core.Dtos;
using Core.ServicesAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApi.Hubs;

[Authorize]
public class CommentHub(IUserQuestGrpcService userQuestGrpcService,
                        ICommentService commentService,
                        ILogger<CommentHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var planId = Context.GetHttpContext()?.Request.Query["planId"].ToString();

        if (string.IsNullOrEmpty(planId))
        {
            logger.LogWarning("CommentHub connection rejected - Missing planId - ConnectionId: {ConnectionId}", connectionId);
            Context.Abort();
            return;
        }

        var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        try
        {
            var isInPlan = await userQuestGrpcService.IsUserInPlan(userId!, planId!);

            if (isInPlan)
            {
                Context.Items["planId"] = planId;

                await Groups.AddToGroupAsync(connectionId, planId!);
            }
            else
            {
                logger.LogWarning("CommentHub connection rejected - User not in plan - UserId: {UserId}, PlanId: {PlanId}, ConnectionId: {ConnectionId}", userId, planId, connectionId);
                Context.Abort();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CommentHub connection error during gRPC validation - UserId: {UserId}, PlanId: {PlanId}, ConnectionId: {ConnectionId}", userId, planId, connectionId);
            Context.Abort();
        }
    }

    public async Task CreateComment(AddCommentDto addCommentDto)
    {
        var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        addCommentDto.UserId = userId;
        addCommentDto.PlanId = Context.Items["planId"] as string;
        var result = await commentService.AddCommentAsync(addCommentDto);
        if (result.IsSuccess)
        {
            await Clients.Group(addCommentDto.PlanId!).SendAsync("ReceiveComment", result.Value);
        }
    }
    public async Task EditComment(UpdateCommentDto updateCommentDto)
    {
        var userId = "e";
        var planId = Context.Items["planId"] as string;
        updateCommentDto.UserId = userId;
        var result = await commentService.UpdateCommentAsync(updateCommentDto);
        await result.MapAsync(
            onSuccess: comment => Clients.Group(planId!).SendAsync("ReceiveEditedComment", comment),
            onFailure: errors => Clients.Caller.SendAsync("ReceiveErrors", errors)
        );
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            logger.LogWarning(exception, "CommentHub client disconnected with exception - ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            logger.LogInformation("CommentHub client disconnected gracefully - ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

