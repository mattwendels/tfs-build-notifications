using System;
using System.Linq;
using System.Threading;
using Tfs.BuildNotifications.Core.Extensions;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Services
{
    public class PollingService : IPollingService
    {
        private readonly IBuildConfigurationService _buildConfigurationService;

        private Timer _timer;
        private DateTime _lastNotificationCheckTime = DateTime.Now;

        public delegate void BuildNotificationEvent(Build build);
        public delegate void BuildPollCompletedEvent(bool hasFailedBuilds);

        public event BuildNotificationEvent OnBuildStatusChange;
        public event BuildPollCompletedEvent OnBuildPollComplete;

        public TimeSpan PollInterval { get; set; }

        public PollingService(IBuildConfigurationService buildConfigurationService)
        {
            _buildConfigurationService = buildConfigurationService;
        }

        public void PollBuildNotifications()
        {
            _timer = new Timer(ProccessNotifications);

            StartTimer(TimeSpan.FromSeconds(0));
        }

        #region Private Methods

        private void ProccessNotifications(object state)
        {
            StopTimer();

            var buildUpdates = _buildConfigurationService.GetLastBuildPerDefinition();

            foreach (var build in buildUpdates)
            {
                if ((build.InProgress && build.StartTime > _lastNotificationCheckTime) ||
                    (!build.InProgress && build.LastFinished > _lastNotificationCheckTime))
                {
                    OnBuildStatusChange?.Invoke(build);
                }
            }

            OnBuildPollComplete?.Invoke(buildUpdates.Any(b => b.Result.ToBuildResult() == BuildResult.Failed));

            _lastNotificationCheckTime = DateTime.Now;

            StartTimer(PollInterval);
        }

        private void StartTimer(TimeSpan delay)
        {
            _timer.Change(delay, PollInterval);
        }

        private void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #endregion
    }
}
