using Nancy;
using System.Linq;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Web.ViewModels;

namespace Tfs.BuildNotifications.Web.Nancy.Modules
{
    public class HomeModule : NancyModule
    {
        private readonly IBuildConfigurationService _buildConfigurationService;

        public HomeModule(IBuildConfigurationService buildConfigurationService)
        {
            _buildConfigurationService = buildConfigurationService;

            Get["/"] = _ =>
            {
                if (!_buildConfigurationService.HasConfiguration())
                {
                    return Response.AsRedirect("/addconnection");
                }

                var buildConfig = _buildConfigurationService.GetBuildConfig();
                var viewModel = new HomeViewModel();

                foreach (var connection in buildConfig.Connections)
                {
                    var dashboardConnection = new DashboardConnection { Connection = connection };

                    if (!connection.Broken)
                    {
                        foreach (var project in connection.Projects)
                        {
                            var dashboardProject = new DashboardProject { Project = project };

                            foreach (var buildDef in project.BuildDefinitions)
                            {
                                dashboardProject.BuildDefinitions.Add(new DashboardBuildDefinition
                                {
                                    Name = buildDef.Name,
                                    Url = buildDef.Url,
                                    LocalId = buildDef.LocalId,

                                    Builds = _buildConfigurationService.GetBuilds(connection, project.Name,
                                        buildDef.Id).ToList()
                                });
                            }

                            dashboardConnection.Projects.Add(dashboardProject);
                        }
                    }

                    viewModel.Connections.Add(dashboardConnection);
                }

                return View["Views/Index.cshtml", viewModel];
            };

            Get["/buildsummary"] = x =>
            {
                var buildConfig = _buildConfigurationService.GetBuildConfig();

                var connectionId = (string)Request.Query["connectionId"];
                var projectId = (string)Request.Query["projectId"];
                var localBuildId = (string)Request.Query["localBuildId"];

                var connection = buildConfig.Connections
                    .FirstOrDefault(c => c.Id.ToString() == connectionId);

                var project = connection?.Projects.FirstOrDefault(p => p.Id.ToString() == projectId);

                var buildDef = project?.BuildDefinitions
                    .FirstOrDefault(b => b.LocalId.ToString() == localBuildId);

                if (buildDef != null)
                {
                    var model = new SingleBuildDefinitionViewModel
                    {
                        Connection = connection,
                        Project = project,

                        BuildDefinition = new DashboardBuildDefinition
                        {
                            Name = buildDef.Name,
                            Url = buildDef.Url,
                            LocalId = buildDef.LocalId,

                            Builds = _buildConfigurationService.GetBuilds(connection, project.Name,
                                    buildDef.Id).ToList()
                        }
                    };

                    return View["Views/Shared/_BuildSummary.cshtml", model];
                }
                else
                {
                    return null;
                }
            };
        }
    }
}
