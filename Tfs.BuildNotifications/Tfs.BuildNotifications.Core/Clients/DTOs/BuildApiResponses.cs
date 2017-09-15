using RestSharp.Deserializers;
using System;
using System.Collections.Generic;

namespace Tfs.BuildNotifications.Core.Clients.DTOs
{
    public class BuildDefinitionApiResponseWrapper
    {
        [DeserializeAs(Name = "value")]
        public List<BuildDefinitionApiResponse> BuildDefinitions { get; set; }
    }

    public class BuildApiResponseWrapper
    {
        [DeserializeAs(Name = "value")]
        public List<BuildApiResponse> Builds { get; set; }
    }

    public class BuildApiResponse
    {
        [DeserializeAs(Name = "id")]
        public int BuildRunId { get; set; }

        [DeserializeAs(Name = "status")]
        public string Status { get; set; }

        [DeserializeAs(Name = "result")]
        public string Result { get; set; }

        [DeserializeAs(Name = "startTime")]
        public DateTime? StartTime { get; set; }

        [DeserializeAs(Name = "finishTime")]
        public DateTime? FinishTime { get; set; }

        [DeserializeAs(Name = "url")]
        public string Url { get; set; }

        [DeserializeAs(Name = "definition")]
        public BuildDefinitionApiResponse Definition { get; set; }

        [DeserializeAs(Name = "requestedFor")]
        public BuildRequestedByApiReponse Requestor { get; set; }

        [DeserializeAs(Name = "_links")]
        public Links Links { get; set; }
    }

    public class BuildDefinitionApiResponse
    {
        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "url")]
        public string Url { get; set; }

        [DeserializeAs(Name = "id")]
        public int Id { get; set; }

        [DeserializeAs(Name = "_links")]
        public Links Links { get; set; }
    }

    public class BuildRequestedByApiReponse
    {
        [DeserializeAs(Name = "displayName")]
        public string Name { get; set; }

        [DeserializeAs(Name = "imageUrl")]
        public string ProfileImageUrl { get; set; }
    }

    public class Links
    {
        [DeserializeAs(Name = "web")]
        public WebLink Web { get; set; }
    }

    public class WebLink
    {
        [DeserializeAs(Name = "href")]
        public string Url { get; set; }
    }
}
