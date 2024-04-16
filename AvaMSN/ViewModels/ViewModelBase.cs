using AvaMSN.Utils;
using AvaMSN.Views;
using ReactiveUI;

namespace AvaMSN.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public static NotificationManager? NotificationManager { get; set; }
    protected static SettingsWindow? SettingsWindow { get; set; }
}
