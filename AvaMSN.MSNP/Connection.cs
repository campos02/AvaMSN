using System.Net.Sockets;
using System.Net;
using System.Text;
using AvaMSN.MSNP.Utils;

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
    /// Resolves host address and stablishes a connection to it.
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
    }

    /// <summary>
    /// Asynchronously sends a binary message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected async Task SendAsync(byte[] message)
    {
        await Client!.SendAsync(message, SocketFlags.None);
    }

    /// <summary>
    /// Waits for a message from the server and returns it as a string.
    /// </summary>
    /// <returns>Message received in string format.</returns>
    protected async Task<string> ReceiveStringAsync()
    {
        receiveSource.Cancel();
        receiveSource = new CancellationTokenSource();
        receiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);

        return Encoding.UTF8.GetString(buffer, 0, received);
    }

    /// <summary>
    /// Waits for a message from the server and returns it.
    /// </summary>
    /// <returns>Message received.</returns>
    protected async Task<byte[]> ReceiveAsync()
    {
        receiveSource.Cancel();
        receiveSource = new CancellationTokenSource();
        receiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);

        byte[] response = new byte[received];
        Buffer.BlockCopy(buffer, 0, response, 0, received);

        return response;
    }

    /// <summary>
    /// Continuously receives and handles incoming messages.
    /// </summary>
    /// <returns></returns>
    protected async Task ReceiveIncomingAsync()
    {
        receiveSource.Cancel();
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

            HandleIncoming(response);
        }
    }

    /// <summary>
    /// Virtual function to handle incoming messages that aren't the result of a command.
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
            // Send ping
            var message = "PNG\r\n";
            
            try
            {
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
            await SendAsync("OUT\r\n");
            DisconnectSocket(requested);
        }
    }

    /// <summary>
    /// Disconnects the socket and invokes the Disconnected event.
    /// </summary>
    /// <param name="requested">Whether the disconnection was requested by the user.</param>
    protected void DisconnectSocket(bool requested = true)
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
    }
}
