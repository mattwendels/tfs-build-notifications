using System;

namespace Tfs.BuildNotifications.Model
{
    public enum TfsServerDeployment
    {
        OnPremises,
        OnlineVsts
    }

    public static class TfsServerDeploymentHelper
    {
        public static TfsServerDeployment ToTfsServerDeploymentHelperEnum(string value)
        {
            return (TfsServerDeployment)Enum.Parse(typeof(TfsServerDeployment), value);
        }

        public static string ToFriendlyName(this TfsServerDeployment tfsServerDeployment)
        {
            switch (tfsServerDeployment)
            {
                case TfsServerDeployment.OnlineVsts:
                    return "Visual Studio Online";

                case TfsServerDeployment.OnPremises:
                    return "On Premises";

                default:
                    return "Unknown";
            }
        }
    }
}
