using Nancy;
using Nancy.ErrorHandling;
using Nancy.Extensions;
using Nancy.ViewEngines;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;

namespace Tfs.BuildNotifications.Web.Nancy.Handlers
{
    public class ServerErrorHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public ServerErrorHandler(IViewFactory factory) : base(factory) { }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var e = context.GetException();

            LoggingErrorHandler.LogService.Log($"Nancy server error.", e);

            var response = RenderView(context, "Views/Error.cshtml", e);

            response.StatusCode = HttpStatusCode.InternalServerError;

            context.Response = response;
        }
    }

    public static class LoggingErrorHandler
    {
        public static ILogService LogService;
    }
}
