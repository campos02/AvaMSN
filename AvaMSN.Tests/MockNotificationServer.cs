using System.Net.Sockets;
using System.Net;
using System.Text;

namespace AvaMSN.Tests
{
    /// <summary>
    /// Runs a local simulated Notification Server for testing purposes.
    /// </summary>
    internal class MockNotificationServer
    {
        private CancellationTokenSource receiveSource = new CancellationTokenSource();
        private Socket? listener;
        private Socket? handler;

        /// <summary>
        /// Creates a new listening socket and binds it to localhost.
        /// </summary>
        /// <returns></returns>
        public async Task BindSocket()
        {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 1863);

            listener = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(100);
        }

        /// <summary>
        /// Accepts a new connection.
        /// </summary>
        /// <returns></returns>
        public async Task AcceptAsync()
        {
            handler = await listener!.AcceptAsync();
        }

        /// <summary>
        /// Replys to USR I with a "nonce".
        /// </summary>
        /// <returns></returns>
        public async Task ReplyNonce()
        {
            await receiveSource.CancelAsync();
            receiveSource = new CancellationTokenSource();
            receiveSource.CancelAfter(30000);

            while (true)
            {
                var buffer = new byte[1664];
                var received = await handler!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                if (response.Contains("USR") && response.Contains("SSO I"))
                {
                    string TransactionID = response.Split(" ")[1];
                    var echoBytes = Encoding.UTF8.GetBytes($"USR {TransactionID} SSO S MBI_KEY_OLD AAA4545s\r\n");
                    await handler.SendAsync(echoBytes, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// Simulates the server rejecting a return value by replying with the 911 error.
        /// </summary>
        /// <returns></returns>
        public async Task RejectReturnValue()
        {
            await ReplyNonce();
            while (true)
            {
                var buffer = new byte[1664];
                var received = await handler!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                if (response.Contains("USR") && response.Contains("SSO S"))
                {
                    string TransactionID = response.Split(" ")[1];
                    var echoBytes = Encoding.UTF8.GetBytes($"911 {TransactionID}\r\n");
                    await handler.SendAsync(echoBytes, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// Simulates the server accepting a return value and authenticating the user.
        /// </summary>
        /// <returns></returns>
        public async Task AcceptReturnValue()
        {
            await ReplyNonce();
            while (true)
            {
                var buffer = new byte[1664];
                var received = await handler!.ReceiveAsync(buffer, SocketFlags.None, receiveSource.Token);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                if (response.Contains("USR") && response.Contains("SSO S"))
                {
                    string TransactionID = response.Split(" ")[1];
                    var echoBytes = Encoding.UTF8.GetBytes($"USR {TransactionID} OK test@example.com 1 0\r\n");
                    await handler.SendAsync(echoBytes, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// Cancels receiving tasks and closes sockets.
        /// </summary>
        public void CloseSocket()
        {
            receiveSource.Cancel();
            receiveSource.Dispose();
            listener?.Close();
            handler?.Close();
        }
    }
}
