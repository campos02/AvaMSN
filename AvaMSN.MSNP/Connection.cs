﻿using System.Net.Sockets;
using System.Net;
using System.Text;

namespace AvaMSN.MSNP;

public class Connection
{
    public Socket Client { get; private set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1863;
    public int TransactionID { get; protected set; }
    public bool Connected { get; private set; }

    public event EventHandler? Disconnected;

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
        Client.Send(messageBytes, SocketFlags.None);
    }

    protected async Task SendAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await Client.SendAsync(messageBytes, SocketFlags.None);
    }

    protected async Task<string> ReceiveAsync()
    {
        ReceiveSource.Cancel();
        ReceiveSource = new CancellationTokenSource();
        ReceiveSource.CancelAfter(5000);

        var buffer = new byte[1664];
        var received = await Client.ReceiveAsync(buffer, SocketFlags.None, ReceiveSource.Token);

        return Encoding.UTF8.GetString(buffer, 0, received);
    }

    protected async Task ReceiveIncomingAsync()
    {
        ReceiveSource.Cancel();
        ReceiveSource = new CancellationTokenSource();

        try
        {
            while (true)
            {
                var buffer = new byte[1160];
                var received = await Client.ReceiveAsync(buffer, SocketFlags.None, ReceiveSource.Token);

                string response = Encoding.UTF8.GetString(buffer, 0, received);
                HandleIncoming(response);
            }
        }
        catch (OperationCanceledException) { return; }
    }

    protected virtual object HandleIncoming(string response) => response switch
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

            string response;
            try
            {
                // Receive version
                response = await ReceiveAsync();
            }
            catch (OperationCanceledException) { break; }

            if (response.StartsWith("QNG"))
            {
                Connected = true;
                break;
            }
        }
    }

    /// <summary>
    /// Send disconnection command
    /// </summary>
    /// <returns></returns>
    public virtual async Task DisconnectAsync()
    {
        await SendAsync("OUT\r\n");
        Client.Shutdown(SocketShutdown.Both);
        Client.Dispose();
        Connected = false;

        Disconnected?.Invoke(this, new EventArgs());
    }
}
