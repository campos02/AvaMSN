using AvaMSN.Models;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaMSN.ViewModels;

public class ConversationViewModel : ViewModelBase
{
    private Conversation? conversation;

    public Conversation? Conversation
    {
        get => conversation;
        set => this.RaiseAndSetIfChanged(ref conversation, value);
    }

    private string message = string.Empty;

    public string Message
    {
        get => message;
        set => this.RaiseAndSetIfChanged(ref message, value);
    }

    public ReactiveCommand<Unit, Unit> SendCommand { get; }
    public ReactiveCommand<Unit, Unit> NudgeCommand { get; }
    public ReactiveCommand<Unit, Unit> TypingUserCommand { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public ReactiveCommand<Unit, Unit> CompleteHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteHistoryCommand { get; }

    public Database? Database { get; set; }

    public event EventHandler? BackButtonPressed;

    public ConversationViewModel() 
    {
        SendCommand = ReactiveCommand.CreateFromTask(Send);
        NudgeCommand = ReactiveCommand.CreateFromTask(SendNudge);
        TypingUserCommand = ReactiveCommand.CreateFromTask(SendTypingUser);
        BackCommand = ReactiveCommand.Create(Back);

        CompleteHistoryCommand = ReactiveCommand.Create(GetCompleteHistory);
        DeleteHistoryCommand = ReactiveCommand.Create(DeleteHistory);
    }

    private async Task Send()
    {
        if (Conversation == null)
            return;

        await Conversation.SendTextMessage(Message);
        Message = string.Empty;
    }

    private async Task SendTypingUser()
    {
        if (Conversation == null)
            return;

        await Conversation.SendTypingUser();
    }

    private async Task SendNudge()
    {
        if (Conversation == null)
            return;

        await Conversation.SendNudge();
    }

    private void Back()
    {
        BackButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void GetCompleteHistory()
    {
        Conversation?.GetHistory();
    }

    private void DeleteHistory()
    {
        Conversation?.DeleteHistory();
    }
}
