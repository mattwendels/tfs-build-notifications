using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tfs.BuildNotifications.Common.Telemetry;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;
using Tfs.BuildNotifications.Core.Clients.DTOs;
using Tfs.BuildNotifications.Core.Clients.Interfaces;
using Tfs.BuildNotifications.Core.Exceptions;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Clients
{
    public class TfsApiClient : ITfsApiClient
    {
        private const string _buildsApiVersion = "2.0";
        private const string _defaultApiVersion = "1.0";

        private readonly ILogService _logService;

        public TfsApiClient(ILogService logService)
        {
            _logService = logService;
        }

        public void TestConnection(string tfsServerUrl, TfsServerDeployment deployment, string userName, string password, 
            string personalAccessToken)
        {
            var connection = new Connection
            {
                TfsServerDeployment = deployment,
                UserName = userName,
                Password = password,
                PersonalAccessToken = personalAccessToken,
                TfsServerUrl = tfsServerUrl
            };

            TestConnection(connection);
        }

        public void TestConnection(Connection connection)
        {
            this.DoApiRequest<List<Project>>(connection, $"_apis/projects?api-version={_defaultApiVersion}");
        }

        public IEnumerable<Project> GetProjects(Connection connection)
        {
            return this.DoApiRequest<ProjectListApiResponse>(connection, $"_apis/projects?api-version={_defaultApiVersion}").Projects;
        }

        public IEnumerable<BuildDefinition> GetBuildDefinitions(Connection connection, string projectName)
        {
            var result = this.DoApiRequest<BuildDefinitionApiResponseWrapper>(connection, $"{projectName}/_apis/build/definitions?api-version={_buildsApiVersion}")
                .BuildDefinitions.OrderBy(b => b.Name);

            var buildDefinitions = new List<BuildDefinition>();

            foreach (var item in result)
            {
				// XAML builds have no links collection. These are ignored (and are marked for deprecation in TFS anyway).
				if (item.Links != null)
				{
					buildDefinitions.Add(new BuildDefinition
					{
						Id = item.Id,
						Name = item.Name,
						Url = item.Links.Web.Url
					});
				}
            }

            return buildDefinitions;
        }

        public List<Build> GetBuilds(Connection connection, string projectName, int buildDefinitionId)
        {
            var apiBuildResult = this.DoApiRequest<BuildApiResponseWrapper>(connection,
                $"{projectName}/_apis/build/builds?api-version={_buildsApiVersion}&definitions={buildDefinitionId}&$top=10");

            return ConvertToBuildModel(apiBuildResult);
        }

        public List<Build> GetLastBuildPerDefinition(Connection connection, string projectName, List<int> buildDefinitionIds)
        {
            var apiBuildResult = this.DoApiRequest<BuildApiResponseWrapper>(connection,
                $"{projectName}/_apis/build/builds?api-version={_buildsApiVersion}&definitions={string.Join(",", buildDefinitionIds)}&maxBuildsPerDefinition=1");

            return ConvertToBuildModel(apiBuildResult);
        }

        #region Private Methods

        private T DoApiRequest<T>(Connection connection, string resource) where T : new()
        {
            var credentials = GetCredentials(connection);
            
            var client = new RestClient(connection.TfsServerUrl);

            client.Authenticator = new NtlmAuthenticator(credentials.UserName, credentials.Password);

            var request = new RestRequest(resource);

            var response = client.Execute<T>(request);

            var content = response.Content;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logService.Log($"Failed to connect to TFS server {connection.TfsServerUrl}. Response: {response.StatusCode}", 
                    LogLevel.Error);

                throw new ServiceValidationException(
                    $"Unable to connect to the specified TFS server. Server response: {response.StatusCode}");
            }

            return response.Data;
        }

        private (string UserName, string Password) GetCredentials(Connection connection)
        {
            var basicAuthUserName = "";
            var basicAuthPassword = "";

            if (connection.TfsServerDeployment == TfsServerDeployment.OnPremises)
            {
                basicAuthUserName = connection.UserName;
                basicAuthPassword = connection.Password;
            }
            else if (connection.TfsServerDeployment == TfsServerDeployment.OnlineVsts)
            {
                basicAuthPassword = connection.PersonalAccessToken;
            }

            return (basicAuthUserName, basicAuthPassword);
        }

        private List<Build> ConvertToBuildModel(BuildApiResponseWrapper apiBuildResult)
        {
            var builds = new List<Build>();

            foreach (var apiBuild in apiBuildResult.Builds)
            {
                builds.Add(new Build
                {
                    DefinitionName = apiBuild.Definition.Name,
                    LastFinished = apiBuild.FinishTime,
                    LastRequestByProfileImageUrl = apiBuild.Requestor.ProfileImageUrl,
                    LastRequestedBy = apiBuild.Requestor.Name,
                    Result = apiBuild.Result,
                    StartTime = apiBuild.StartTime,
                    Status = apiBuild.Status,
                    Url = apiBuild.Links.Web.Url,
                    BuildRunId = apiBuild.BuildRunId
                });
            }

            return builds;
        }

        #endregion
    }
}
