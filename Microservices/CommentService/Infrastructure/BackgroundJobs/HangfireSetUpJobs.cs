using Core.ServicesAbstraction;
using Hangfire;
using System.Linq.Expressions;

namespace Infrastructure.BackgroundJobs;

public class HangfireSetUpJobs : IBackgroundService
{
    public void ExecuteInBackground(Expression<Action> job)
    {
        BackgroundJob.Enqueue(job);
    }
}
