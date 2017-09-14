using System.Collections.Generic;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Web.ViewModels
{
    public class AddBuildsViewModel : ViewModelBase
    {
        public Connection Connection { get; set; }

        public Project Project { get; set; }

        public List<BuildSelectionModel> BuildSelections { get; set; }
    }

    public class BuildSelectionModel
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool Selected { get; set; }
        public bool Disabled { get; set; }
    }
}
