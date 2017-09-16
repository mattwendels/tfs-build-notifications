using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Tfs.BuildNotifications.Web.Nancy.Configuration
{
    public class NancyCustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            // Allow custom error handlers in release configuration.
            // https://github.com/NancyFx/Nancy/issues/2052
            StaticConfiguration.DisableErrorTraces = false;

            base.ApplicationStartup(container, pipelines);
        }
    }
}
