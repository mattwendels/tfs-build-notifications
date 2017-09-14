using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Web.ViewModels
{
    public class SingleBuildDefinitionViewModel : HomeViewModel
    {
        public Connection Connection { get; set; }

        public Project Project { get; set; }

        public DashboardBuildDefinition BuildDefinition { get; set; }
    }
}
