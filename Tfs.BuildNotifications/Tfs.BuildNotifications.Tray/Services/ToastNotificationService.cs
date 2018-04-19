using System;
using System.IO;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Services.Interfaces;
using Windows.UI.Notifications;

namespace Tfs.BuildNotifications.Tray.Services
{
	public class ToastNotificationService : NotificationService, INotificationService
    {
        public ToastNotificationService(IAppConfig appConfig) : base(appConfig) { }

        protected override void ShowNotification(string title, string message, string image, Action onActivation = null)
        {
			var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

			var stringElements = toastXml.GetElementsByTagName("text");

			stringElements[0].AppendChild(toastXml.CreateTextNode(title));
			stringElements[1].AppendChild(toastXml.CreateTextNode(message));

			// Specify the absolute path to an image.
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

        private void ToastActivated(ToastNotification sender, object e, Action action)
        {
            action.Invoke();
        }
    }
}
