using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Practices.Unity;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Properties;
using Tfs.BuildNotifications.Tray.Services.Interfaces;

namespace Tfs.BuildNotifications.Tray
{
    internal interface ITrayIconApplicationContext
    {
        void UpdateTrayStatus(bool hasAnyFailedBuilds);
    }

    internal class TrayIconApplicationContext : ApplicationContext, ITrayIconApplicationContext
    {
        public static NotifyIcon TrayIcon;

        private readonly INotificationService _notificationService;
        private readonly IBuildConfigurationService _buildConfigurationService;
        private readonly IAppConfig _appConfig;

        public TrayIconApplicationContext([Dependency("Tray")]INotificationService notificationService,
            IBuildConfigurationService buildConfigurationService, IAppConfig appConfig) 
        {
            _notificationService = notificationService;
            _buildConfigurationService = buildConfigurationService;
            _appConfig = appConfig;

            TrayIcon = new NotifyIcon()
            {
                Icon = Resources.TfsIcon,
                Visible = true,
                Text = "TFS Build Notifications",

                ContextMenu = new ContextMenu(new MenuItem[] 
                {
                    new MenuItem("Open Dashboard", OpenDashboard),
                    new MenuItem("Exit", Exit)
                })
            };

            TrayIcon.MouseClick += OnMouseClick;

            if (_buildConfigurationService.HasAnyMonitoredBuilds())
            {
                _notificationService.ShowGenericNotification("TFS Build Notifications", "Build notifications enabled.",
                    () => Process.Start($"http://localhost:{_appConfig.WebsitePort}"));
            }
            else
            {
                _notificationService.ShowGenericNotification("TFS Build Notifications", 
                    "No build notifications configured. Click here to get started.", 
                    () => Process.Start($"http://localhost:{_appConfig.WebsitePort}"));
            }
        }

        public void UpdateTrayStatus(bool hasAnyFailedBuilds)
        {
            if (hasAnyFailedBuilds)
            {
                TrayIcon.Icon = Resources.TfsIconBuildsFailing;
                TrayIcon.Text = "TFS Build Notifications - Build(s) failing";
            }
            else
            {
                TrayIcon.Icon = Resources.TfsIcon;
                TrayIcon.Text = "TFS Build Notifications";
            }
        }

        #region Events

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it.
            TrayIcon.Visible = false;

            Application.Exit();
        }

        void OpenDashboard(object sender, EventArgs e)
        {
            Process.Start($"http://localhost:{_appConfig.WebsitePort}");
        }

        void OnApplicationExit(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
        }

        void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);

                mi.Invoke(TrayIcon, null);
            }
        }

        #endregion
    }
}
