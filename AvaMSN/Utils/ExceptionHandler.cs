using System;
using AvaMSN.Utils.Notifications;
using Serilog;

namespace AvaMSN.Utils;

/// <summary>
/// Custom reactive command exception handler.
/// </summary>
public class ExceptionHandler : IObserver<Exception>
{
    public NotificationHandler NotificationHandler { get; } = new NotificationHandler();

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
#if DEBUG
        NotificationHandler.ShowError(error.ToString());
#else
        NotificationHandler.ShowError(error.Message);
#endif
        Log.Error("Got a {Exception}: {Message}", nameof(error), error.Message);
    }

    public void OnNext(Exception exception)
    {
#if DEBUG
        NotificationHandler.ShowError(exception.ToString());
#else
        NotificationHandler.ShowError(exception.Message);
#endif
        Log.Error("Got a {Exception}: {Message}", nameof(exception), exception.Message);
    }
}
