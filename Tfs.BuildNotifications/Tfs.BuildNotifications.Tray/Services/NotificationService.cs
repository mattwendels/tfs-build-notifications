using System;
using System.Diagnostics;
using Tfs.BuildNotifications.Core.Extensions;
using Tfs.BuildNotifications.Model;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;

namespace Tfs.BuildNotifications.Tray.Services
{
	public abstract class NotificationService
	{
		protected const string DefaultNotificationIcon = "Content/Images/tfs-logo.png";
		protected const string BuildStartedIcon = "Content/Images/play.png";
		protected const string BuildCompletedIcon = "Content/Images/success.png";
		protected const string BuildFailedIcon = "Content/Images/failed.png";
		protected const string BuildStoppedIcon = "Content/Images/stopped.png";
		protected const string BuildStatusUnknownIcon = "Content/Images/unknown.png";

		private readonly IAppConfig _appConfig;

		public NotificationService(IAppConfig appConfig)
		{
			_appConfig = appConfig;
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

		protected abstract void ShowNotification(string title, string message, string image, Action onActivation = null);

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
		
		#endregion
	}
}
