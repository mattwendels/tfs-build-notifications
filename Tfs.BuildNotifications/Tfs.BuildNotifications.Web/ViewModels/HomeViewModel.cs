using System;
using System.Collections.Generic;
using System.Linq;
using Tfs.BuildNotifications.Common.Extensions;
using Tfs.BuildNotifications.Core.Extensions;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Web.ViewModels
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Connections = new List<DashboardConnection>();
        }

        public List<DashboardConnection> Connections { get; set; }

        public string GetHistoryCssClassName(Build build)
        {
            if (build.InProgress)
            {
                return "history-running";
            }

            switch (build.Result.ToBuildResult())
            {
                case BuildResult.Succeeded:
                    return "history-success";

                case BuildResult.Failed:
                    return "history-fail";

                default:
                    return "history-other";
            }
        }

        public bool AnyBuildsConfigured => 
            Connections.Any(c => !c.Connection.Broken && c.Projects.Any(p => p.BuildDefinitions.Any()));
    }

    public class DashboardConnection
    {
        public DashboardConnection()
        {
            Projects = new List<DashboardProject>();
        }

        public Connection Connection { get; set; }

        public List<DashboardProject> Projects { get; set; }
    }

    public class DashboardProject
    {
        public DashboardProject()
        {
            BuildDefinitions = new List<DashboardBuildDefinition>();
        }

        public Project Project { get; set; }

        public List<DashboardBuildDefinition> BuildDefinitions { get; set; }
    }

    public class DashboardBuildDefinition
    {
        public DashboardBuildDefinition()
        {
            Builds = new List<Build>();
        }

        public string Name { get; set; }
        public string ShortName => Name.Shorten(40, "...");

        public List<Build> Builds { get; set; }

        public bool IsRunning => Builds.Any(b => b.InProgress);

        public string LastRequestedBy => Builds.FirstOrDefault()?.LastRequestedBy;
        public string LastFinished => Builds.FirstOrDefault()?.LastFinished?.ToString() ?? "N/A";
        public string LastStarted => Builds.FirstOrDefault()?.StartTime?.ToString() ?? "Unknown";
        public string Url { get; set; }

        public Guid LocalId { get; set; }

        public BuildResult Status => Builds.FirstOrDefault()?.Result.ToBuildResult() ?? BuildResult.Unknown;

        public bool RequiresAttention => Status == BuildResult.Failed || Status == BuildResult.Stopped;

        public string StatusImageFileName
        {
            get
            {
                if (IsRunning)
                {
                    return "play.png";
                }

                switch (Status)
                {
                    case BuildResult.Succeeded:
                        return "success.png";

                    case BuildResult.Failed:
                        return "failed.png";

                    case BuildResult.Stopped:
                        return "stopped.png";

                    default:
                        return "unknown.png";
                }
            }
        }
    }
}
