namespace Core.ServicesAbstraction;

public interface IUserQuestGrpcService
{
    Task<bool> IsUserInPlan(string userId, string planId);
}
