using Core.ServicesAbstractions;

namespace WebApi.Tests;

/// <summary>
/// Test implementation of IGrpcTeamService that verifies user exists in team by default
/// </summary>
public class TestGrpcTeamService : IGrpcTeamService
{
    public async Task<bool> IsUserExist(string userId, string teamId)
    {
        // Simulate async operation
        await Task.Delay(10);

        // Return true by default to allow tests to pass
        // Individual tests can override this behavior by mocking the service directly
        return !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(teamId);
    }
}
