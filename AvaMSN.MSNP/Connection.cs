using System.Net.Sockets;
using System.Net;
using System.Text;
using AvaMSN.MSNP.Models;
using Serilog;
using AvaMSN.MSNP.SOAP;

namespace AvaMSN.MSNP;

/// <summary>
/// Base class that represents a connection to a server implementing MSNP.
/// </summary>
public abstract class Connection
{
    private Socket? Client { get; set; }
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 1863;
    internal int TransactionID { get; set; }
    public bool Connected { get; private set; }

    public event EventHandler<DisconnectedEventArgs>? Disconnected;
    private CancellationTokenSource receiveSource = new CancellationTokenSource();

    /// <summary>
    /// Resolves host address and establishes a connection to it.
    /// </summary>
    /// <returns></returns>
    public virtual async Task Connect()
    {
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(Host);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint ipEndPoint = new(ipAddress, Port);

        Client = new Socket(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        await Client.ConnectAsync(ipEndPoint);
        Connected = true;
    }

    /// <summary>
    /// Asynchronously sends a text command.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    internal async Task SendAsync(string command)
    {
        if (Client == null)
            throw new NullReferenceException("Socket is null");
        
        var messageBytes = Encoding.UTF8.GetBytes(command);
        await Client.SendAsync(messageBytes, SocketFlags.None);
        Log.Information("Sent: {Message}", command);
    }

    /// <summary>
    /// Asynchronously sends a binary command.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    internal async Task SendAsync(byte[] command)
    {
        if (Client == null)
            throw new NullReferenceException("Socket is null");
        
        await Client.SendAsync(command, SocketFlags.None);
        Log.Information("Sent: {Message}", Encoding.UTF8.GetString(command));
    }

    /// <summary>
    /// Waits for a response from the server and returns it as a string.
    /// </summary>
    /// <returns>Message received in string format.</returns>
    internal async Task<string> ReceiveStringAsync()
    {
        if (Client == null)
            throw new NullReferenceException("Socket is null");
        
        await receiveSource.CancelAsync();
        receiveSource = new CancellationTokenSource();
        receiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        
        Log.Information("Received: {Response}", response);
        return response;
    }

    /// <summary>
    /// Waits for a response from the server and returns it.
    /// </summary>
    /// <returns>Message received.</returns>
    internal async Task<byte[]> ReceiveAsync()
    {
        if (Client == null)
            throw new NullReferenceException("Socket is null");
        
        await receiveSource.CancelAsync();
        receiveSource = new CancellationTokenSource();
        receiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
        byte[] response = new byte[received];
        Buffer.BlockCopy(buffer, 0, response, 0, received);
        
        Log.Information("Received: {Response}", Encoding.UTF8.GetString(response));
        return response;
    }

    /// <summary>
    /// Continuously receives and handles incoming responses.
    /// </summary>
    /// <returns></returns>
    public async Task ReceiveIncomingAsync()
    {
        if (Client == null)
            throw new NullReferenceException("Socket is null");
        
        await receiveSource.CancelAsync();
        receiveSource = new CancellationTokenSource();

        while (true)
        {
            var buffer = new byte[1664];
            int received;

            try
            {
                received = await Client.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
            }
            catch (OperationCanceledException) { return; }

            byte[] response = new byte[received];
            Buffer.BlockCopy(buffer, 0, response, 0, received);
            
            Log.Information("Incoming: {Response}", Encoding.UTF8.GetString(response));
            await HandleIncoming(response);
        }
    }

    /// <summary>
    /// Virtual function to handle incoming responses that aren't the result of a command.
    /// </summary>
    /// <param name="response">Message received.</param>
    /// <returns></returns>
    internal abstract Task HandleIncoming(byte[] response);

    /// <summary>
    /// Pings the server every 30 seconds so connection isn't automatically closed.
    /// </summary>
    /// <returns></returns>
    public async Task StartPinging()
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
                DisconnectSocket(new DisconnectedEventArgs { Requested = false });
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
    /// <param name="eventArgs">Disconnection event arguments.</param>
    /// <returns></returns>
    public virtual async Task DisconnectAsync(DisconnectedEventArgs? eventArgs = null)
    {
        eventArgs ??= new DisconnectedEventArgs
        {
            Requested = true
        };

        if (Connected)
        {
            await SendAsync("OUT\r\n");
            DisconnectSocket(eventArgs);
        }
    }

    /// <summary>
    /// Disconnects the socket and invokes the Disconnected event.
    /// </summary>
    /// <param name="eventArgs">Disconnection event arguments.</param>
    /// <exception cref="NullReferenceException">Thrown if the socket is null.</exception>
    private void DisconnectSocket(DisconnectedEventArgs eventArgs)
    {
        if (Client == null)
            throw new NullReferenceException("Socket is null");
        
        receiveSource.Cancel();
        receiveSource.Dispose();

        Client.Shutdown(SocketShutdown.Both);
        Client.Dispose();
        Connected = false;

        Disconnected?.Invoke(this, eventArgs);
        
        if (!eventArgs.Requested)
            Log.Error("Connection to {Server} on port {Port} has been lost", Host, Port);
        else
            Log.Information("Disconnected from {Server} on port {Port}", Host, Port);
    }
}
