using System;
using System.Collections.ObjectModel;
using AvaMSN.Models;

namespace AvaMSN.ViewModels.Design;

public class DesignConversationWindowViewModel : ConversationWindowViewModel
{
    public DesignConversationWindowViewModel()
    {
        Conversation = new Conversation(new Contact()
        {
            DisplayName = "test",
            Presence = "Online"
        },
        new Profile()
        {
            DisplayName = "test"
        })
        {
            MessageHistory = new ObservableCollection<Message>
            {
                new Message()
                {
                    SenderDisplayName = "test",
                    DateTime = DateTime.Now,
                    Text = "test message",
                }
            },

            Messages = new ObservableCollection<Message>
            {
                new Message()
                {
                    SenderDisplayName = "test",
                    DateTime = DateTime.Now,
                    Text = "test message",
                },
                new Message()
                {
                    SenderDisplayName = "test",
                    DateTime = DateTime.Now,
                    Text = "test message",
                    Bold = true,
                    Italic = true,
                    Decorations = "Strikethrough Underline"
                }
            },

            TypingUser = true
        };
        Message = "Test message";
    }
}
