using System;

namespace Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces
{
    public interface IAppConfig
    {
        int WebsitePort { get; }

        TimeSpan NotificationIntervalMinutes { get; }

        bool UseToolTipNotifications { get; }

        bool NotifyNonSuccessfulBuildsOnly { get; }
    }
}
