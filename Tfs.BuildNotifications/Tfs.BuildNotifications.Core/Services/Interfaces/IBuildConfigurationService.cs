using System;
using System.Collections.Generic;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Services.Interfaces
{
    public interface IBuildConfigurationService
    {
        bool HasConfiguration();

        bool HasAnyMonitoredBuilds();

        BuildConfig GetBuildConfig();

        void TestAllConnections();

        void AddConnection(string tfsServerUrl, string tfsLocation, string userName, string password,
            string personalAccessToken);

        void EditConnection(string connectionId, string tfsServerUrl, string tfsLocation, string userName, string password,
            string personalAccessToken);

        IEnumerable<Project> GetProjects(string connectionId, out Connection connection);

        void AddProjects(string connectionId, List<Project> projectsSelected);

        IEnumerable<BuildDefinition> GetBuildDefinitions(string connectionId, string projectId,
            out Connection connection, out Project project);

        void AddBuildDefinitions(string connectionId, string projectId, List<BuildDefinition> buildsSelected);

        IEnumerable<Build> GetBuilds(Connection connection, string projectName, int buildDefinitionId);

        IEnumerable<Build> GetLastBuildPerDefinition();

        bool ProjectExistsInConfig(Guid connectionId, Guid projectId);

        bool BuildExistsInConfig(Guid connectionId, Guid projectId, int buildId);

        void DeleteConnection(string connectionId);

        void DeleteProject(string connectionId, string projectId);

        void DeleteBuild(string localId);
    }
}
