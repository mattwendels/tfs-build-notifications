using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Web.Services.Interfaces
{
    public interface IWebsiteDashboardService
    {
        void StartWebsite();
        void UpdateDashboardBuildStatus(Build build);
    }
}
