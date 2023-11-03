using AvaMSN.Models;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaMSN.ViewModels;

public class ConversationViewModel : ConnectedViewModelBase
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

    public Database? Database { get; set; }

    public ConversationViewModel() 
    {
        SendCommand = ReactiveCommand.CreateFromTask(Send);
        NudgeCommand = ReactiveCommand.CreateFromTask(SendNudge);
        TypingUserCommand = ReactiveCommand.CreateFromTask(SendTypingUser);
        BackCommand = ReactiveCommand.Create(Back);
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
        Chatting = false;
    }
}
