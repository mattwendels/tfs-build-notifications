using System;
using Tfs.BuildNotifications.Model;

namespace Tfs.BuildNotifications.Tray.Services.Interfaces
{
    public interface INotificationService
    {
        void ShowGenericNotification(string title, string message, Action onActivation = null);

        void NotifyBuildChange(Build build);
    }
}
