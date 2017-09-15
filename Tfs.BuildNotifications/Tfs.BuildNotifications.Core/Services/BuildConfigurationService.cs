using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Tfs.BuildNotifications.Common.Encryption;
using Tfs.BuildNotifications.Common.Helpers.Interfaces;
using Tfs.BuildNotifications.Core.Clients.Interfaces;
using Tfs.BuildNotifications.Core.Exceptions;
using Tfs.BuildNotifications.Core.Services.Interfaces;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Services
{
    public class BuildConfigurationService : IBuildConfigurationService
    {
        private const string ConfigFileName = "builds-config.json";
        private const string EncryptionKeyRegistrySubKey = @"Software\TFSBuildNotifications\Data";
        private const string EncryptionKeyName = "EncKey";

        private readonly ITfsApiClient _tfsApiClient;
        private readonly IRegistryHelper _registryHelper;

        public BuildConfigurationService(ITfsApiClient tfsApiClient, IRegistryHelper registryHelper)
        {
            _tfsApiClient = tfsApiClient;
            _registryHelper = registryHelper;
        }

        public BuildConfig GetBuildConfig()
        {
            return GetConfig();
        }

        public bool HasConfiguration()
        {
            return File.Exists(ConfigFileName);
        }

        public bool HasAnyMonitoredBuilds()
        {
            if (!HasConfiguration())
            {
                return false;
            }

            return GetConfig().Connections.Any(c => !c.Broken && c.Projects.Any(p => p.BuildDefinitions.Any()));
        }

        public void Init()
        {
            // Set key in registry for encrypting passwords saved in config file.
            if (!_registryHelper.KeyExistsWithValue(EncryptionKeyRegistrySubKey, EncryptionKeyName))
            {
                var aesProvider = new AesCryptoServiceProvider();

                aesProvider.KeySize = 256;
                aesProvider.GenerateKey();

                _registryHelper.SetValue(EncryptionKeyRegistrySubKey, EncryptionKeyName, Convert.ToBase64String(aesProvider.Key));
            }

            // Test connections.
            var config = GetConfig();

            if (config.Connections.Any())
            {
                foreach (var connection in config.Connections)
                {
                    try
                    {
                        _tfsApiClient.TestConnection(DecryptConnection(connection));

                        connection.Broken = false;
                        connection.LastConnectionError = null;
                    }
                    catch (ServiceValidationException s)
                    {
                        connection.Broken = true;
                        connection.LastConnectionError = s.ServiceErrors.First();
                    }
                }

                SaveConfig(config);
            }
        }

        public void AddConnection(string tfsServerUrl, string tfsLocation, string userName, string password, 
            string personalAccessToken)
        {
            ValidateConnection(tfsServerUrl, tfsLocation, userName, password, personalAccessToken, out var deployment);

            var config = GetConfig();

            if (config.Connections.Any(c => c.TfsServerUrl.Trim().ToLower() == tfsServerUrl.Trim().ToLower()))
            {
                throw new ServiceValidationException("A connection to this server has already been added.");
            }

            var newConnection = new Connection
            {
                Password = EncryptString(password),
                PersonalAccessToken = EncryptString(personalAccessToken),
                TfsServerDeployment = deployment,
                TfsServerUrl = tfsServerUrl,
                UserName = userName,
                Id = Guid.NewGuid()
            };

            config.Connections.Add(newConnection);

            SaveConfig(config);
        }

        public void EditConnection(string connectionId, string tfsServerUrl, string tfsLocation, string userName, string password,
            string personalAccessToken)
        {
            var config = GetConfig();
            var connection = config.Connections.First(c => c.Id.ToString() == connectionId);
                
            ValidateConnection(tfsServerUrl, tfsLocation, userName, password, personalAccessToken, out var deployment);

            connection.Password = EncryptString(password);
            connection.PersonalAccessToken = EncryptString(personalAccessToken);
            connection.TfsServerDeployment = deployment;
            connection.TfsServerUrl = tfsServerUrl;
            connection.UserName = userName;
            connection.Broken = false;
            connection.LastConnectionError = null;

            SaveConfig(config);
        }

        public IEnumerable<Project> GetProjects(string connectionId, out Connection connection)
        {
            connection = GetConnection(connectionId);

            return _tfsApiClient.GetProjects(connection);
        }

        public void AddProjects(string connectionId, List<Project> projectsSelected)
        {
            var config = GetConfig();
            var allProjects = GetProjects(connectionId, out var connection);

            // Remove any projects that don't exist in this connection.
            projectsSelected.RemoveAll(p => !allProjects.Select(ap => ap.Id).Contains(p.Id));

            var configConnection = config.Connections.First(c => c.Id == connection.Id);

            // Remove any projects already added to this connection config.
            projectsSelected.RemoveAll(p => configConnection.Projects.Select(cp => cp.Id).Contains(p.Id));

            configConnection.Projects.AddRange(projectsSelected);

            SaveConfig(config);
        }

        public IEnumerable<BuildDefinition> GetBuildDefinitions(string connectionId, string projectId,
            out Connection connection, out Project project)
        {
            project = this.GetProject(projectId, connectionId, out connection);

            return _tfsApiClient.GetBuildDefinitions(connection, project.Name);
        }

        public void AddBuildDefinitions(string connectionId, string projectId, List<BuildDefinition> buildsSelected)
        {
            var config = GetConfig();

            var project = this.GetProject(projectId, connectionId, out var connection);

            var projectBuildDefinitions = _tfsApiClient.GetBuildDefinitions(connection, project.Name);

            // Remove any builds that don't belong to the selected project.
            buildsSelected.RemoveAll(b => !projectBuildDefinitions.Select(s => s.Id).Contains(b.Id));

            var configProject = config.Connections
                .First(c => c.Id.ToString() == connectionId).Projects.First(p => p.Id.ToString() == projectId);

            // Remove any builds already added to this project config.
            buildsSelected.RemoveAll(b => configProject.BuildDefinitions.Select(cb => cb.Id).Contains(b.Id));

            // Store definition URL and a unique ID.
            foreach (var def in buildsSelected)
            {
                def.Url = projectBuildDefinitions.First(b => b.Id == def.Id).Url;
                def.LocalId = Guid.NewGuid();
            }

            configProject.BuildDefinitions.AddRange(buildsSelected);

            SaveConfig(config);
        }

        public IEnumerable<Build> GetBuilds(Connection connection, string projectName, int buildDefinitionId)
        {
            return _tfsApiClient.GetBuilds(DecryptConnection(connection), projectName, buildDefinitionId);
        }

        public IEnumerable<Build> GetLastBuildPerDefinition()
        {
            var config = GetConfig();
            var builds = new List<Build>();

            foreach (var connection in config.Connections.Where(c => !c.Broken))
            {
                foreach (var project in connection.Projects)
                {
                    var lastBuilds = _tfsApiClient.GetLastBuildPerDefinition(DecryptConnection(connection), project.Name,
                            project.BuildDefinitions.Select(b => b.Id).ToList());

                    foreach (var build in lastBuilds)
                    {
                        build.DefinitionLocalId =
                            project.BuildDefinitions.FirstOrDefault(b => b.Name == build.DefinitionName)?.LocalId ?? Guid.Empty;

                        builds.Add(build);
                    }
                }
            }

            return builds;
        }

        public bool ProjectExistsInConfig(Guid connectionId, Guid projectId)
        {
            return GetConfig().Connections.Any(c => c.Id == connectionId && c.Projects.Any(p => p.Id == projectId));
        }

        public bool BuildExistsInConfig(Guid connectionId, Guid projectId, int buildId)
        {
            return GetConfig().Connections
                .Any(c => c.Id == connectionId && c.Projects.Any(p => p.Id == projectId && p.BuildDefinitions.Select(b => b.Id)
                .Contains(buildId)));
        }

        public void DeleteConnection(string connectionId)
        {
            var config = GetConfig();

            config.Connections.RemoveAll(c => c.Id.ToString() == connectionId);

            SaveConfig(config);
        }

        public void DeleteProject(string connectionId, string projectId)
        {
            var config = GetConfig();

            var connection = config.Connections.FirstOrDefault(c => c.Id.ToString() == connectionId);

            if (connection != null)
            {
                connection.Projects.RemoveAll(p => p.Id.ToString() == projectId);

                SaveConfig(config);
            }
        }

        public void DeleteBuild(string localId)
        {
            var config = GetConfig();

            var project = config.Connections.SelectMany(c => c.Projects)
                .FirstOrDefault(p => p.BuildDefinitions.Any(b => b.LocalId.ToString() == localId));

            if (project != null)
            {
                project.BuildDefinitions.RemoveAll(b => b.LocalId.ToString() == localId);

                SaveConfig(config);
            }
        }

        #region Private Methods

        private BuildConfig GetConfig()
        {
            if (!HasConfiguration())
            {
                return new BuildConfig();
            }
            else
            {
                // ToDo: error handling, invalid config etc.
                using (var reader = new StreamReader(ConfigFileName))
                {
                    return JsonConvert.DeserializeObject<BuildConfig>(reader.ReadToEnd());
                }
            }
        }

        private void SaveConfig(BuildConfig config)
        {
            var stringConfig = JsonConvert.SerializeObject(config);

            using (var streamWriter = new StreamWriter(ConfigFileName, false))
            {
                streamWriter.Write(stringConfig);
            }
        }

        private Connection GetConnection(string connectionId)
        {
            var connection = GetConfig().Connections.FirstOrDefault(c => c.Id.ToString() == connectionId);

            if (connection == null)
            {
                throw new InvalidOperationException($"Connection not found '{connectionId}");
            }

            return DecryptConnection(connection);
        }

        private Project GetProject(string projectId, string connectionId, out Connection connection)
        {
            var allProjects = GetProjects(connectionId, out connection);

            var project = connection.Projects.FirstOrDefault(p => p.Id.ToString() == projectId);

            if (project == null)
            {
                throw new InvalidOperationException($"Project not found '{projectId}");
            }

            return project;
        }

        private void ValidateConnection(string tfsServerUrl, string tfsLocation, string userName, string password,
            string personalAccessToken, out TfsServerDeployment deployment)
        {
            deployment = TfsServerDeploymentHelper.ToTfsServerDeploymentHelperEnum(tfsLocation);

            if (deployment == TfsServerDeployment.OnPremises)
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    throw new ServiceValidationException("Please enter a user name");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ServiceValidationException("Please enter a password");
                }
            }
            else if (deployment == TfsServerDeployment.OnlineVsts)
            {
                if (string.IsNullOrWhiteSpace(personalAccessToken))
                {
                    throw new ServiceValidationException("Please enter your VSTS personal access token");
                }
            }

            try
            {
                _tfsApiClient.TestConnection(tfsServerUrl, deployment, userName, password, personalAccessToken);
            }
            catch
            {
                throw new ServiceValidationException($"Unable to connect to the specified TFS server.");
            }
        }

        private string EncryptString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var key = _registryHelper.GetValue<string>(EncryptionKeyRegistrySubKey, EncryptionKeyName);

            return EncryptionService.AesEncryptString(key, value);
        }

        private string DecryptString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var key = _registryHelper.GetValue<string>(EncryptionKeyRegistrySubKey, EncryptionKeyName);

            return EncryptionService.AesDecryptString(key, value);
        }

        private Connection DecryptConnection(Connection connection)
        {
            var clone = (Connection)connection.Clone();

            clone.Password = DecryptString(clone.Password);
            clone.PersonalAccessToken = DecryptString(clone.PersonalAccessToken);

            return clone;
        }

        #endregion
    }
}
