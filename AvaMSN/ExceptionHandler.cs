using System;

namespace AvaMSN;

/// <summary>
/// Custom reactive command exception handler.
/// </summary>
public class ExceptionHandler : IObserver<Exception>
{
    public NotificationManager NotificationManager { get; set; } = new NotificationManager();

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
#if DEBUG
        NotificationManager.ShowError(error.ToString());
#else
        NotificationManager.ShowError(error.Message);
#endif
    }

    public void OnNext(Exception exception)
    {
#if DEBUG
        NotificationManager.ShowError(exception.ToString());
#else
        NotificationManager.ShowError(exception.Message);
#endif
    }
}
