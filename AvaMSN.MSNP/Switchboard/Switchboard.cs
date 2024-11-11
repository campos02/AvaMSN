using System.Text;
using System.Timers;
using AvaMSN.MSNP.Models;
using AvaMSN.MSNP.Switchboard.Messaging;
using Timer = System.Timers.Timer;
using Serilog;

namespace AvaMSN.MSNP.Switchboard;

/// <summary>
/// Represents a connection to the Switchboard (SB).
/// </summary>
public class Switchboard : Connection
{
    public User? User { get; init; }
    public Contact? Contact { get; init; }
    public bool ContactInSession { get; internal set; }
    internal IncomingMessaging? IncomingMessaging { get; set; }
    private readonly Timer timeout;

    public Switchboard()
    {
        // Set 10 minute timeout
        timeout = new Timer(600000)
        {
            AutoReset = false
        };
        timeout.Elapsed += Timer_Elapsed;
    }

    public override async Task Connect()
    {
        await base.Connect();
        ResetTimeout();
        Log.Information("Connected to SB {Server} on port {Port}", Host, Port);
    }

    /// <summary>
    /// Starts or restarts the timeout timer.
    /// </summary>
    internal void ResetTimeout()
    {
        timeout.Stop();
        timeout.Start();
    }
    
    /// <summary>
    /// Overload taking a string response.
    /// </summary>
    /// <param name="response">String response.</param>
    internal async Task HandleIncoming(string response)
    {
        await HandleIncoming(Encoding.UTF8.GetBytes(response));
    }

    /// <summary>
    /// Handles responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Incoming binary response.</param>
    /// <returns></returns>
    internal override async Task HandleIncoming(byte[] response)
    {
        if (IncomingMessaging != null)
            await IncomingMessaging.HandleIncoming(response);
        
        string responseString = Encoding.UTF8.GetString(response);
        string command = responseString.Split(" ")[0];

        await (command switch
        {
            "BYE" => DisconnectAsync(),
            _ => Task.CompletedTask
        });
    }

    public override async Task DisconnectAsync(bool requested = true)
    {
        await base.DisconnectAsync(requested);
        timeout.Stop();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Log.Information("SB {Server} on port {Port} has timed out", Host, Port);
        await DisconnectAsync();
    }
}
