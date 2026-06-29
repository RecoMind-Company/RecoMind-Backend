namespace Core.ServicesAbstraction;

public interface IQuestGrpcService
{
    Task<bool> IsTaskExists(string taskId);
}
