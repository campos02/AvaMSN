using AvaMSN.MSNP.Exceptions;
using AvaMSN.MSNP.NotificationServer;
using AvaMSN.MSNP.NotificationServer.Authentication;

namespace AvaMSN.Tests;

public class AuthTest
{
    [Fact]
    public async Task Create_New_Connection_On_911_Error()
    {
        MockNotificationServer server = new MockNotificationServer();
        await server.BindSocket();
        _ = server.AcceptAsync();

        NotificationServer notificationServer = new NotificationServer
        {
            Host = "localhost",
            User =
            {
                Email = "test@example.com"
            }
        };

        await notificationServer.Connect();
        Authentication authentication = new Authentication(notificationServer);

        _ = server.RejectReturnValue();
        try
        {
            await authentication.AuthenticateWithTicket();
        }
        catch (MsnpServerAuthException)
        {
            _ = server.AcceptAsync();
            notificationServer = new NotificationServer
            {
                Host = "localhost",
                User =
                {
                    Email = "test@example.com"
                }
            };

            await notificationServer.Connect();
            authentication = new Authentication(notificationServer);

            _ = server.AcceptReturnValue();

            // The actual client will try to authenticate with a password instead here,
            // this is only meant to test if creating a new connection works.
            await authentication.AuthenticateWithTicket();
        }

        server.CloseSocket();
    }
}
