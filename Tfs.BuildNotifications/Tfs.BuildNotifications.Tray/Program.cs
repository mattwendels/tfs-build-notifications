using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Tray.Infrastructure.Unity;
using Tfs.BuildNotifications.Tray.Services.Interfaces;
using Tfs.BuildNotifications.Web.Services.Interfaces;

namespace Tfs.BuildNotifications.Tray
{
    static class Program
    {
        private static ILogService _logService;

        static void Main()
        {
            var procName = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == procName) > 1)
            {
                MessageBox.Show("The TFS Build Notification application is already running.", "Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = new Bootstrapper().Bootstrap();

            _logService = container.Resolve<ILogService>();

            var pollingService = container.Resolve<IPollingService>();
            var dashboardWebsite = container.Resolve<IWebsiteDashboardService>();
            var notificationServices = container.ResolveAll<INotificationService>();
            var buildConfigService = container.Resolve<IBuildConfigurationService>();
            var tray = container.Resolve<ITrayIconApplicationContext>();
            
            buildConfigService.Init();

            pollingService.OnBuildPollComplete += tray.UpdateTrayStatus;
            pollingService.OnBuildStatusChange += dashboardWebsite.UpdateDashboardBuildStatus;

            foreach (var service in notificationServices)
            {
                pollingService.OnBuildStatusChange += service.NotifyBuildChange;
            }
            
            pollingService.PollBuildNotifications();

            dashboardWebsite.StartWebsite();

			Application.Run((ApplicationContext)tray);
        }

        #region Private Methods

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            _logService.Log("Unhandled application error", exception);

            MessageBox.Show($"TFS Build Notifications Error\r\n\r\nSorry, something seems to have gone wrong...\r\n\r\n{exception}\r\n\r\nPlease restart the application.", 
                "TFS Build Notifications - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void RegisterNotifications()
        {
            // ToDo: Fix and tidy up.
            var key = Registry.CurrentUser
                .CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\\TFS Build Notifications");

            key.SetValue("ShowInActionCenter", 1);
            key.Close();
        }

        #endregion
    }
}
