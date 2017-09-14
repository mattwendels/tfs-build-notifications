using Nancy.ViewEngines.Razor;
using System.Collections.Generic;

namespace Tfs.BuildNotifications.Web.Nancy.Configuration
{
    public class NancyRazorConfiguration : IRazorConfiguration
    {
        public bool AutoIncludeModelNamespace => true;

        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Tfs.BuildNotifications.Web";
            yield return "Tfs.BuildNotifications.Model";
            yield return "Tfs.BuildNotifications.Core";
            yield return "Tfs.BuildNotifications.Common";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Nancy.Validation";
            yield return "System.Globalization";
            yield return "System.Collections.Generic";
            yield return "System.Linq";
            yield return "Tfs.BuildNotifications.Web.ViewModels";
            yield return "Tfs.BuildNotifications.Model";
            yield return "Tfs.BuildNotifications.Core.Extensions";
            yield return "Tfs.BuildNotifications.Common.Extensions";
        }
    }
}
