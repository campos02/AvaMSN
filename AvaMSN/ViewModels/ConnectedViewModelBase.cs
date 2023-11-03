using ReactiveUI;

namespace AvaMSN.ViewModels;

public class ConnectedViewModelBase : ViewModelBase
{
    private bool loggedIn;

    public bool LoggedIn
    {
        get => loggedIn;
        set => this.RaiseAndSetIfChanged(ref loggedIn, value);
    }

    private bool chatting;

    public bool Chatting
    {
        get => chatting;
        set => this.RaiseAndSetIfChanged(ref chatting, value);
    }
}
