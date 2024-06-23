using AvaMSN.Utils.Notifications;
using AvaMSN.Views;
using ReactiveUI;

namespace AvaMSN.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public static NotificationHandler? NotificationHandler { get; set; }
    protected static SettingsWindow? SettingsWindow { get; set; }
}
