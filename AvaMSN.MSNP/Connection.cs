using System.Net.Sockets;
using System.Net;
using System.Text;
using AvaMSN.MSNP.Utils;
using Serilog;

namespace AvaMSN.MSNP;

/// <summary>
/// Base class that represents a connection to a server implementing MSNP.
/// </summary>
public class Connection
{
    public Socket? Client { get; private set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1863;
    public int TransactionID { get; protected set; }
    public bool Connected { get; private set; }

    public event EventHandler<DisconnectedEventArgs>? Disconnected;
    private CancellationTokenSource receiveSource = new CancellationTokenSource();

    /// <summary>
    /// Resolves host address and establishes a connection to it.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task Connect()
    {
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(Host);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint ipEndPoint = new(ipAddress, Port);

        Client = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        await Client.ConnectAsync(ipEndPoint);
        Connected = true;
    }

    /// <summary>
    /// Sends a text message.
    /// </summary>
    /// <param name="message"></param>
    protected void Send(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        Client?.Send(messageBytes, SocketFlags.None);
        Log.Information("Sent: {Message}", message);
    }

    /// <summary>
    /// Asynchronously sends a text message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected async Task SendAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await Client!.SendAsync(messageBytes, SocketFlags.None);
        Log.Information("Sent: {Message}", message);
    }

    /// <summary>
    /// Asynchronously sends a binary message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected async Task SendAsync(byte[] message)
    {
        await Client!.SendAsync(message, SocketFlags.None);
        Log.Information("Sent: {Message}", Encoding.UTF8.GetString(message));
    }

    /// <summary>
    /// Waits for a response from the server and returns it as a string.
    /// </summary>
    /// <returns>Message received in string format.</returns>
    protected async Task<string> ReceiveStringAsync()
    {
        await receiveSource.CancelAsync();
        receiveSource = new CancellationTokenSource();
        receiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        
        Log.Information("Received: {Response}", response);
        return response;
    }

    /// <summary>
    /// Waits for a response from the server and returns it.
    /// </summary>
    /// <returns>Message received.</returns>
    protected async Task<byte[]> ReceiveAsync()
    {
        await receiveSource.CancelAsync();
        receiveSource = new CancellationTokenSource();
        receiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
        byte[] response = new byte[received];
        Buffer.BlockCopy(buffer, 0, response, 0, received);
        
        Log.Information("Received: {Response}", Encoding.UTF8.GetString(response));
        return response;
    }

    /// <summary>
    /// Continuously receives and handles incoming responses.
    /// </summary>
    /// <returns></returns>
    protected async Task ReceiveIncomingAsync()
    {
        await receiveSource.CancelAsync();
        receiveSource = new CancellationTokenSource();

        while (true)
        {
            var buffer = new byte[1664];
            int received;

            try
            {
                received = await Client!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
            }
            catch (OperationCanceledException) { return; }

            byte[] response = new byte[received];
            Buffer.BlockCopy(buffer, 0, response, 0, received);
            
            Log.Information("Incoming: {Response}", Encoding.UTF8.GetString(response));
            HandleIncoming(response);
        }
    }

    /// <summary>
    /// Virtual function to handle incoming responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Message received.</param>
    /// <returns></returns>
    protected virtual object HandleIncoming(byte[] response) => response switch
    {
        _ => ""
    };

    /// <summary>
    /// Pings the server every 30 seconds so connection isn't automatically closed.
    /// </summary>
    /// <returns></returns>
    public async Task Ping()
    {
        while (true)
        {
            var message = "PNG\r\n";
            try
            {
                // Send ping
                await SendAsync(message);
            }
            catch (SocketException)
            {
                // Shutdown socket and invoke event if connection has been lost
                DisconnectSocket(requested: false);
                break;
            }
            catch (ObjectDisposedException)
            {
                // Stop if socket has been disposed
                break;
            }

            await Task.Delay(30000);
        }
    }

    /// <summary>
    /// Sends a disconnection command and disconnects the socket.
    /// </summary>
    /// <param name="requested">Whether the disconnection was requested by the user.</param>
    /// <returns></returns>
    public virtual async Task DisconnectAsync(bool requested = true)
    {
        if (Connected)
        {
            Log.Information("Sending disconnection command to {Server} on port {Port}", Host, Port);
            await SendAsync("OUT\r\n");
            DisconnectSocket(requested);
        }
    }

    /// <summary>
    /// Disconnects the socket and invokes the Disconnected event.
    /// </summary>
    /// <param name="requested">Whether the disconnection was requested by the user.</param>
    private void DisconnectSocket(bool requested = true)
    {
        receiveSource.Cancel();
        receiveSource.Dispose();

        Client?.Shutdown(SocketShutdown.Both);
        Client?.Dispose();
        Connected = false;

        Disconnected?.Invoke(this, new DisconnectedEventArgs()
        {
            Requested = requested
        });
        
        if (!requested)
            Log.Error("Connection to {Server} on port {Port} has been lost", Host, Port);
        else
            Log.Information("Disconnected from {Server} on port {Port}", Host, Port);
    }
}
