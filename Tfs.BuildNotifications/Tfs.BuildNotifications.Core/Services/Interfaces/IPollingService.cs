using static Tfs.BuildNotifications.Core.Services.PollingService;

namespace Tfs.BuildNotifications.Core.Services.Interfaces
{
    public interface IPollingService
    {
        void PollBuildNotifications();

        event BuildNotificationEvent OnBuildStatusChange;
        event BuildPollCompletedEvent OnBuildPollComplete;
    }
}
