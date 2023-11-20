using System;
using System.Collections.ObjectModel;
using AvaMSN.Models;

namespace AvaMSN.ViewModels.Design;

public class DesignConversationViewModel : ConversationViewModel
{
    public DesignConversationViewModel()
    {
        Conversation = new Conversation(new Contact()
        {
            DisplayName = "test",
            Presence = "Available"
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
                    SenderDisplayName= "test",
                    DateTime = DateTime.Now,
                    Text = "test message",
                },
                new Message()
                {
                    SenderDisplayName= "test",
                    DateTime = DateTime.Now,
                    Text = "test message",
                }
            },

            TypingUser = true
        };
    }
}
