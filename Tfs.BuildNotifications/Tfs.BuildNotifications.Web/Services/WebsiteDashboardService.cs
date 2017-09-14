using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;
using Tfs.BuildNotifications.Model;
using Tfs.BuildNotifications.Web.Nancy.Handlers;
using Tfs.BuildNotifications.Web.Services.Interfaces;
using Tfs.BuildNotifications.Web.SignalR.Interfaces;

namespace Tfs.BuildNotifications.Web.Services
{
    public class WebsiteDashboardService : IWebsiteDashboardService
    {
        public int WebsitePort { get; set; }

        private IDisposable _webApp;

        private readonly IDashboardHub _dashboardHub;
        private readonly ILogService _logService;

        public WebsiteDashboardService(IDashboardHub dashboardHub, ILogService logService)
        {
            _dashboardHub = dashboardHub;
            _logService = logService;

            LoggingErrorHandler.LogService = _logService;
        }

        public void StartWebsite()
        {
            var options = new StartOptions();

            options.Urls.Add($"http://{Environment.MachineName}:{WebsitePort}");
            options.Urls.Add($"http://127.0.0.1:{WebsitePort}");
            options.Urls.Add($"http://localhost:{WebsitePort}");

            _webApp = WebApp.Start<Startup>(options);

#if DEBUG
            Process.Start($"http://localhost:{WebsitePort}");
#endif
            _logService.Log($"Dashboard website started on port {WebsitePort}.");
        }

        public void UpdateDashboardBuildStatus(Build build)
        {
            _dashboardHub.NotifyBuildChange(build);
        }
    }
}
