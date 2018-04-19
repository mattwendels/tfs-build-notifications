using System;
using System.Windows.Forms;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Services.Interfaces;

namespace Tfs.BuildNotifications.Tray.Services
{
	public class ToolTipNotificationService : NotificationService, INotificationService
	{
		public ToolTipNotificationService(IAppConfig appConfig) : base(appConfig) { }

		protected override void ShowNotification(string title, string message, string image, Action onActivation = null)
		{
			TrayIconApplicationContext.TrayIcon.ShowBalloonTip(0, title, message, GetToolTipIcon(image));
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
	}
}
