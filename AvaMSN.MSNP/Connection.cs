using System.Net.Sockets;
using System.Net;
using System.Text;

namespace AvaMSN.MSNP;

public class Connection
{
    public Socket? Client { get; private set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1863;
    public int TransactionID { get; protected set; }
    public bool Connected { get; private set; }

    public event EventHandler<DisconnectedEventArgs>? Disconnected;

    public CancellationTokenSource ReceiveSource { get; set; } = new CancellationTokenSource();

    protected async Task Connect()
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

    protected void Send(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        Client!.Send(messageBytes, SocketFlags.None);
    }

    protected async Task SendAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await Client!.SendAsync(messageBytes, SocketFlags.None);
    }

    protected async Task SendAsync(byte[] message)
    {
        await Client!.SendAsync(message, SocketFlags.None);
    }

    protected async Task<string> ReceiveStringAsync()
    {
        ReceiveSource.Cancel();
        ReceiveSource = new CancellationTokenSource();
        ReceiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, ReceiveSource.Token);

        return Encoding.UTF8.GetString(buffer, 0, received);
    }

    protected async Task<byte[]> ReceiveAsync()
    {
        ReceiveSource.Cancel();
        ReceiveSource = new CancellationTokenSource();
        ReceiveSource.CancelAfter(30000);

        var buffer = new byte[1664];
        var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, ReceiveSource.Token);

        byte[] response = new byte[received];
        Buffer.BlockCopy(buffer, 0, response, 0, received);
        return response;
    }

    protected async Task ReceiveIncomingAsync()
    {
        ReceiveSource.Cancel();
        ReceiveSource = new CancellationTokenSource();

        try
        {
            while (true)
            {
                var buffer = new byte[1664];
                var received = await Client!.ReceiveAsync(buffer, SocketFlags.None, ReceiveSource.Token);

                byte[] response = new byte[received];
                Buffer.BlockCopy(buffer, 0, response, 0, received);
                HandleIncoming(response);
            }
        }
        catch (OperationCanceledException) { return; }
    }

    protected virtual object HandleIncoming(byte[] response) => response switch
    {
        _ => ""
    };

    public async Task Ping()
    {
        while (true)
        {
            // Send ping
            var message = "PNG\r\n";
            await SendAsync(message);

            await Task.Delay(30000);
        }
    }

    /// <summary>
    /// Send disconnection command and invoke Disconnected event
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync()
    {
        await SendAsync("OUT\r\n");
        Client!.Shutdown(SocketShutdown.Both);
        Client.Dispose();
        Connected = false;

        Disconnected?.Invoke(this, new DisconnectedEventArgs()
        {
            Requested = true
        });
    }
}
