using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace SmartRetailWpf
{
    internal class ToastNotificationManager
    {
        private Notifier notifier;

        internal ToastNotificationManager()
        {
            notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(2),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        internal void ShowInformation(string message)
        {
            notifier.ShowInformation(message);
        }

        internal void ShowWarning(string message)
        {
            notifier.ShowWarning(message);
        }

        internal void ShowError(string message)
        {
            notifier.ShowError(message);
        }

        internal void ShowSuccess(string message)
        {
            notifier.ShowSuccess(message);
        }
    }
}
