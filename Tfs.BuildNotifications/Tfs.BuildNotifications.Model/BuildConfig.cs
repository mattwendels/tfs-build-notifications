using System;
using System.Collections.Generic;

namespace Tfs.BuildNotifications.Model
{
    public class BuildConfig
    {
        public BuildConfig()
        {
            Connections = new List<Connection>();
        }

        public List<Connection> Connections { get; set; }
    }

    public class Connection
    {
        public Connection()
        {
            Projects = new List<Project>();
        }

        public string TfsServerUrl { get; set; }
        public string PersonalAccessToken { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public TfsServerDeployment TfsServerDeployment { get; set; }

        public List<Project> Projects { get; set; }

        public Guid Id { get; set; }

        public bool Broken { get; set; }

        public string LastConnectionError { get; set; }
    }

    public class Project
    {
        public Project()
        {
            BuildDefinitions = new List<BuildDefinition>();
        }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public List<BuildDefinition> BuildDefinitions { get; set; }
    }

    public class BuildDefinition
    {
        public BuildDefinition() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Guid LocalId { get; set; }
    }
}
