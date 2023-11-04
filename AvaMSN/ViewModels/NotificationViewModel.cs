using AvaMSN.Models;
using ReactiveUI;
using System;
using System.Reactive;

namespace AvaMSN.ViewModels;

public class NotificationViewModel : ViewModelBase
{
    public Contact? Sender { get; set; }
    public Message? Message { get; set; }

    public event EventHandler? ReplyTapped;
    public event EventHandler? DismissTapped;

    public ReactiveCommand<Unit, Unit> ReplyCommand { get; }
    public ReactiveCommand<Unit, Unit> DismissCommand { get; }

    public NotificationViewModel()
    {
        ReplyCommand = ReactiveCommand.Create(Reply);
        DismissCommand = ReactiveCommand.Create(Dismiss);
    }

    public void Reply()
    {
        ReplyTapped?.Invoke(this, EventArgs.Empty);
    }

    public void Dismiss()
    {
        DismissTapped?.Invoke(this, EventArgs.Empty);
    }
}
