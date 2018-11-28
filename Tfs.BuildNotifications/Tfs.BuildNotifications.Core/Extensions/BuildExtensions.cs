using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Extensions
{
    public static class BuildExtensions
    {
        public static BuildResult GetBuildResult(this Build build)
        {
            var buildResult = Model.BuildResult.Unknown;

            switch (build?.Result?.Trim().ToLower())
            {
                case "succeeded":
                    buildResult = BuildResult.Succeeded;
                    break;

                case "failed":
                    buildResult = BuildResult.Failed;
                    break;

                case "canceled":
                    buildResult = BuildResult.Stopped;
                    break;
            }

            if (buildResult == BuildResult.Unknown && build.InProgress)
            {
                buildResult = BuildResult.InProgress;
            }

            return buildResult;
        }
    }
}
