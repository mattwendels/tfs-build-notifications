using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Web.SignalR.Interfaces
{
    public interface IDashboardHub
    {
        void NotifyBuildChange(Build build);
    }
}
