using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System;
using Tfs.BuildNotifications.Common.Helpers;
using Tfs.BuildNotifications.Common.Helpers.Interfaces;
using Tfs.BuildNotifications.Common.Telemetry;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;
using Tfs.BuildNotifications.Core.Clients;
using Tfs.BuildNotifications.Core.Clients.Interfaces;
using Tfs.BuildNotifications.Core.Services;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Tray.Infrastructure.Config;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Services;
using Tfs.BuildNotifications.Tray.Services.Interfaces;
using Tfs.BuildNotifications.Web.Services;
using Tfs.BuildNotifications.Web.Services.Interfaces;
using Tfs.BuildNotifications.Web.SignalR;
using Tfs.BuildNotifications.Web.SignalR.Interfaces;

namespace Tfs.BuildNotifications.Tray.Infrastructure.Unity
{
	public class Bootstrapper
    {
		private ILogService _logService = new LogService();

		public IUnityContainer Bootstrap()
        {
			var container = new UnityContainer();
            var appConfig = new AppConfig();

			if (appConfig.UseToolTipNotifications || !IsWindows10())
			{
				container.RegisterType<INotificationService, ToolTipNotificationService>();
			}
			else
			{
				container.RegisterType<INotificationService, ToastNotificationService>();
			}

			container
                .RegisterType<IAppConfig, AppConfig>()
                .RegisterType<ITrayIconApplicationContext, TrayIconApplicationContext>()
                .RegisterType<ITfsApiClient, TfsApiClient>()
                .RegisterType<IBuildConfigurationService, BuildConfigurationService>()
                .RegisterType<IPollingService, PollingService>(new InjectionProperty("PollInterval", appConfig.NotificationIntervalMinutes))
                .RegisterType<IDashboardHub, DashboardHub>()
                .RegisterType<ILogService, LogService>()
                .RegisterType<IRegistryHelper, RegistryHelper>()
                .RegisterType<IWebsiteDashboardService, WebsiteDashboardService>(new InjectionProperty("WebsitePort", appConfig.WebsitePort));


            return container;
        }

		#region Private Methods

		private bool IsWindows10()
		{
			try
			{
				var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

				var productName = (string)reg.GetValue("ProductName");

				return productName.StartsWith("Windows 10");
			}
			catch (Exception e)
			{
				_logService.Log("Failed to identify operating system version", e);

				return false;
			}
		}

		#endregion
	}
}
