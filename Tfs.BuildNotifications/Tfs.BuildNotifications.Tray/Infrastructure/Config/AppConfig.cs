using System;
using System.Configuration;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;

namespace Tfs.BuildNotifications.Tray.Infrastructure.Config
{
    public class AppConfig : IAppConfig
    {
        public int WebsitePort => Convert.ToInt32(GetApplicationSetting("WebApp.Port"));

        public bool UseToolTipNotifications => Convert.ToBoolean(GetApplicationSetting("ToolTipNotifications.Enabled"));

        public bool NotifyNonSuccessfulBuildsOnly => 
            Convert.ToBoolean(GetApplicationSetting("TrayNotifications.NonSuccessfulBuildsOnly"));

        public TimeSpan NotificationIntervalMinutes => 
            TimeSpan.FromMinutes(Convert.ToInt32(GetApplicationSetting("Notifications.TimerIntervalMinutes")));

        #region Private Methods

        private string GetApplicationSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        #endregion
    }
}
