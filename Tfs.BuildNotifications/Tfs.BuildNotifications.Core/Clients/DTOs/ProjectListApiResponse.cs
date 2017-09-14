using RestSharp.Deserializers;
using System.Collections.Generic;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Clients.DTOs
{
    public class ProjectListApiResponse
    {
        [DeserializeAs(Name = "value")]
        public List<Project> Projects { get; set; }
    }
}
