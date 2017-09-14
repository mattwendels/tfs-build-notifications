using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Core.Extensions
{
    public static class BuildExtensions
    {
        public static BuildResult ToBuildResult(this string s)
        {
            switch (s?.Trim().ToLower())
            {
                case "succeeded":
                    return BuildResult.Succeeded;

                case "failed":
                    return BuildResult.Failed;

                case "canceled":
                    return BuildResult.Stopped;

                default:
                    return BuildResult.Unknown;
            }
        }
    }
}
