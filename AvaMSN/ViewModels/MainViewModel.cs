using ReactiveUI;

namespace AvaMSN.ViewModels;

public class MainViewModel : ViewModelBase
{
    private LoginViewModel loginPage = new LoginViewModel();
    private ViewModelBase currentPage;

    public ViewModelBase CurrentPage
    {
        get => currentPage;
        set => this.RaiseAndSetIfChanged(ref currentPage, value);
    }

    private NotificationViewModel? notificationPage;

    public NotificationViewModel? NotificationPage
    {
        get => notificationPage;
        private set => this.RaiseAndSetIfChanged(ref notificationPage, value);
    }

    public MainViewModel()
    {
        currentPage = loginPage;
    }
}
