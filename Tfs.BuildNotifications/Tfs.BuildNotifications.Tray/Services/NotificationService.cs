using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;
using Tfs.BuildNotifications.Core.Extensions;
using Tfs.BuildNotifications.Model;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Services.Interfaces;
using Windows.UI.Notifications;

namespace Tfs.BuildNotifications.Tray.Services
{
    public class NotificationService : INotificationService
    {
        private const string DefaultNotificationIcon = "Content/Images/tfs-logo.png";
        private const string BuildStartedIcon = "Content/Images/play.png";
        private const string BuildCompletedIcon = "Content/Images/success.png";
        private const string BuildFailedIcon = "Content/Images/failed.png";
        private const string BuildStoppedIcon = "Content/Images/stopped.png";
        private const string BuildStatusUnknownIcon = "Content/Images/unknown.png";

        private bool _useToastNotifications = true;

        private readonly ILogService _logService;
        private readonly IAppConfig _appConfig;

        public NotificationService(ILogService logService, IAppConfig appConfig)
        {
            _logService = logService;
            _appConfig = appConfig;

            if (_appConfig.UseToolTipNotifications)
            {
                _useToastNotifications = false;
            }
        }

        public void ShowGenericNotification(string title, string message, Action onActivation = null)
        {
            ShowNotification(title, message, DefaultNotificationIcon, onActivation);
        }

        public void NotifyBuildChange(Build build)
        {
            if (_appConfig.NotifyNonSuccessfulBuildsOnly)
            {
                if (build.Result.ToBuildResult() != BuildResult.Succeeded)
                {
                    ShowBuildStatusNotification(build);
                }
            }
            else
            {
                if (build.InProgress)
                {
                    ShowBuildStartedNotification(build);
                }
                else
                {
                    ShowBuildStatusNotification(build);
                }
            }
            
        }

        #region Private Methods

        private void ShowBuildStartedNotification(Build build)
        {
            ShowNotification(build.DefinitionName, $"Build started by {build.LastRequestedBy}", BuildStartedIcon,
                () => Process.Start(build.Url));
        }

        private void ShowBuildStatusNotification(Build build)
        {
            if (!build.InProgress)
            {
                var result = build.Result.ToBuildResult();

                Action onClick = () => Process.Start(build.Url);

                switch (result)
                {
                    case BuildResult.Failed:
                        ShowNotification(build.DefinitionName, $"{build.LastRequestedBy} broke the build.", BuildFailedIcon,
                            onClick);
                        break;

                    case BuildResult.Stopped:
                        ShowNotification(build.DefinitionName, "Build stopped. Click for details.", BuildStoppedIcon,
                            onClick);
                        break;

                    case BuildResult.Succeeded:
                        ShowNotification(build.DefinitionName, $"Build succeeded. Requested by {build.LastRequestedBy}",
                            BuildCompletedIcon, onClick);
                        break;

                    default:
                        ShowNotification(build.DefinitionName, "Build status unknown. Click for details.",
                            BuildStatusUnknownIcon, onClick);
                        break;
                }
            }
        }

        private void ShowNotification(string title, string message, string image, Action onActivation = null)
        {
            if (_useToastNotifications)
            {
                try
                {
                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                    var stringElements = toastXml.GetElementsByTagName("text");

                    stringElements[0].AppendChild(toastXml.CreateTextNode(title));
                    stringElements[1].AppendChild(toastXml.CreateTextNode(message));

                    // Specify the absolute path to an image
                    var imagePath = "file:///" + Path.GetFullPath(image);
                    var imageElements = toastXml.GetElementsByTagName("image");

                    imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                    var toast = new ToastNotification(toastXml);

                    if (onActivation != null)
                    {
                        toast.Activated += (sender, e) => ToastActivated(sender, e, onActivation);
                    }

                    ToastNotificationManager.CreateToastNotifier("TFS Build Notifications").Show(toast);
                }
                catch (Exception e)
                {
                    _logService.Log("Failed to show toast notification. Reverting to Windows tooltip for notifications.", e);

                    _useToastNotifications = false;

                    TrayIconApplicationContext.TrayIcon.ShowBalloonTip(0, title, message, GetToolTipIcon(image));
                }
            }
            else
            {
                // Fall back to standard Windows balloon tooltip if toast notifications are not available or have failed.
                TrayIconApplicationContext.TrayIcon.ShowBalloonTip(0, title, message, GetToolTipIcon(image));

            }
        }

        private ToolTipIcon GetToolTipIcon(string image)
        {
            switch (image)
            {
                case BuildStoppedIcon:
                case BuildStatusUnknownIcon:
                    return ToolTipIcon.Info;

                case BuildFailedIcon:
                    return ToolTipIcon.Error;

                default:
                    return ToolTipIcon.Info;
            }
        }

        private void ToastActivated(ToastNotification sender, object e, Action action)
        {
            action.Invoke();
        }

        #endregion
    }
}
