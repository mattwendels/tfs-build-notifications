using System;
using System.Collections.Generic;
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
        private Dictionary<string, Build> _lastSeenBuilds;

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

            if (_lastSeenBuilds != null)
            {
                // When polling after the first time we are interested in new builds or where the InProgress value has changed
                var changedBuilds = buildUpdates.Where(b => !_lastSeenBuilds.ContainsKey(b.Url) || _lastSeenBuilds[b.Url].InProgress != b.InProgress);
                foreach (var changedBuild in changedBuilds)
                {
                    OnBuildStatusChange?.Invoke(changedBuild);

                }
            }
            _lastSeenBuilds = buildUpdates.ToDictionary(b => b.Url);

            OnBuildPollComplete?.Invoke(buildUpdates.Any(b => b.GetBuildResult() == BuildResult.Failed));

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
