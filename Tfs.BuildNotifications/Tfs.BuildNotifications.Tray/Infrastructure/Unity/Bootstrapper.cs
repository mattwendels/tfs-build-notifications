using Microsoft.Practices.Unity;
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
        public IUnityContainer Bootstrap()
        {
            var container = new UnityContainer();
            var appConfig = new AppConfig();

            container
                .RegisterType<IAppConfig, AppConfig>()
                .RegisterType<ITrayIconApplicationContext, TrayIconApplicationContext>()
                .RegisterType<INotificationService, NotificationService>()
                .RegisterType<ITfsApiClient, TfsApiClient>()
                .RegisterType<IBuildConfigurationService, BuildConfigurationService>()
                .RegisterType<IPollingService, PollingService>(new InjectionProperty("PollInterval", appConfig.NotificationIntervalMinutes))
                .RegisterType<IDashboardHub, DashboardHub>()
                .RegisterType<ILogService, LogService>()
                .RegisterType<IWebsiteDashboardService, WebsiteDashboardService>(new InjectionProperty("WebsitePort", appConfig.WebsitePort));

            return container;
        }
    }
}
