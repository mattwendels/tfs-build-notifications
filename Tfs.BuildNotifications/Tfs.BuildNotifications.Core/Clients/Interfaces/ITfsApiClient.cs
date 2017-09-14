using System.Collections.Generic;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Clients.Interfaces
{
    public interface ITfsApiClient
    {
        void TestConnection(string tfsServerUrl, TfsServerDeployment deployment, string userName, string password,
            string personalAccessToken);

        void TestConnection(Connection connection);

        IEnumerable<Project> GetProjects(Connection connection);

        IEnumerable<BuildDefinition> GetBuildDefinitions(Connection connection, string projectName);

        List<Build> GetBuilds(Connection connection, string projectName, int buildDefinitionId);

        List<Build> GetLastBuildPerDefinition(Connection connection, string projectName, List<int> buildDefinitionIds);
    }
}
