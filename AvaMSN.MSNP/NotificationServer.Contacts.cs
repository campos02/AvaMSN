namespace AvaMSN.MSNP;

public partial class NotificationServer : Connection
{
    public async Task ChangeDisplayName()
    {
        await ContactList.ChangeDisplayName();
        await SendPRP();
    }
}
