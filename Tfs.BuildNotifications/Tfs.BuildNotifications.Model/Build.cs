using System;
using Tfs.BuildNotifications.Common.Extensions;

namespace Tfs.BuildNotifications.Model
{
    public class Build
    {
        public string DefinitionName { get; set; }
        public string LastRequestedBy { get; set; }
        public string LastRequestByProfileImageUrl { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public string Url { get; set; }

        public DateTime? LastFinished { get; set; }
        public DateTime? StartTime { get; set; }

        public bool InProgress => Status == "inProgress";

        public Guid DefinitionLocalId { get; set; }

        public string DefinitionShortName => DefinitionName?.Shorten(42, "...");
        public string LastFinishedDateString => LastFinished?.ToString();
        public string StartTimeDateString => StartTime?.ToString();
    }
}
