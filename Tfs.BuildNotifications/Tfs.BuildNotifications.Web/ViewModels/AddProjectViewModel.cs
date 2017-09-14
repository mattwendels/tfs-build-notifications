using System;
using System.Collections.Generic;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Web.ViewModels
{
    public class AddProjectViewModel : ViewModelBase
    {
        public Connection Connection { get; set; }

        public List<ProjectSelectionModel> ProjectSelections { get; set; }
    }

    public class ProjectSelectionModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public bool Selected { get; set; }
        public bool Disabled { get; set; }
    }
}
