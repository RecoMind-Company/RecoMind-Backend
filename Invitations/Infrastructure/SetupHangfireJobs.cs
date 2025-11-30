using Core.Interfaces;
using Hangfire;

namespace Infrastructure;

public static class SetupHangfireJobs
{
    public static void AddHangfireJobs()
    {
        RecurringJob.AddOrUpdate<IInvitationService>("expire-invitations-job", service => service.CheckAndExpireInvitations(), Cron.Hourly);
    }
}
