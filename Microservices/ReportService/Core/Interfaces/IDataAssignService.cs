namespace Core.Interfaces;

public interface IDataAssignService
{
    Task<string> DataAssign(string companyId);
    Task<string> DataAssignResult(string taskId);
}
