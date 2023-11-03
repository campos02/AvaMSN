using System.Text;
using System.Web;

namespace AvaMSN.MSNP;

public partial class Switchboard : Connection
{
    public event EventHandler<MessageEventArgs>? MessageReceived;

    protected override async Task HandleIncoming(string response)
    {
        string command = response.Split(" ")[0];

        await (command switch
        {
            "MSG" => Task.Run(() => HandleMSG(response)),
            "BYE" => HandleBYE(),
            _ => Task.CompletedTask
        });
    }

    private void HandleMSG(string response)
    {
        string[] responses = response.Split("\r\n");
        string[] parameters = responses[0].Split(" ");

        int length = Convert.ToInt32(parameters[3]);

        byte[] totalbytes = Encoding.UTF8.GetBytes(response.Replace(responses[0] + "\r\n", ""));
        byte[] payloadBytes = new Span<byte>(totalbytes, 0, length).ToArray();
        string payload = Encoding.UTF8.GetString(payloadBytes);

        string[] payloadParameters = payload.Split("\r\n");

        if (payloadParameters[1] == "Content-Type: text/x-msmsgscontrol")
        {
            if (payloadParameters[2].Contains("TypingUser"))
            {
                string user = payloadParameters[2].Split(" ")[1];

                MessageReceived?.Invoke(this, new MessageEventArgs()
                {
                    Email = user,
                    TypingUser = true
                });
            }
        }

        if (payloadParameters[1] == "Content-Type: text/x-msnmsgr-datacast")
        {
            if (payloadParameters[3].Contains("ID:"))
            {
                string ID = payloadParameters[3].Split(" ")[1];

                if (ID == "1")
                {
                    MessageReceived?.Invoke(this, new MessageEventArgs()
                    {
                        Email = parameters[1],
                        Message = $"{HttpUtility.UrlDecode(parameters[2])} just sent you a nudge!",
                        IsNudge = true
                    });
                }
            }
        }

        if (payloadParameters[1].Contains("Content-Type: text/plain"))
        {
            MessageReceived?.Invoke(this, new MessageEventArgs()
            {
                Email = parameters[1],
                Message = payloadParameters[4]
            });
        }
    }

    private async Task HandleBYE()
    {
        await DisconnectAsync();
    }
}
