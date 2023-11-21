using ReactiveUI;

namespace AvaMSN.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public static NotificationManager? NotificationManager { get; set; }
}
