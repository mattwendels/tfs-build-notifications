using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.Linq;
using Tfs.BuildNotifications.Core.Clients;
using Tfs.BuildNotifications.Core.Services;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Model;
using Tfs.BuildNotifications.Web.Nancy.Handlers;
using Tfs.BuildNotifications.Web.SignalR.Interfaces;

namespace Tfs.BuildNotifications.Web.SignalR
{
    public class DashboardHub : Hub, IDashboardHub
    {
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<DashboardHub>();

        private readonly IBuildConfigurationService _buildConfigurationService;

        public DashboardHub()
        {
            // ToDo: DI
            _buildConfigurationService = new BuildConfigurationService(new TfsApiClient(LoggingErrorHandler.LogService));
        }

        public void NotifyBuildChange(Build build)
        {
            _hubContext.Clients.All.buildNotification(JsonConvert.SerializeObject(build));
        }

        public void GetRunningBuilds(string connId)
        {
            var runningBuilds = _buildConfigurationService.GetLastBuildPerDefinition().Where(b => b.InProgress);

            foreach (var build in runningBuilds)
            {
                _hubContext.Clients.Client(connId).buildNotification(JsonConvert.SerializeObject(build));
            }
        }
    }
}
