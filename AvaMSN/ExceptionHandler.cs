using System;

namespace AvaMSN;

public class ExceptionHandler : IObserver<Exception>
{
    public NotificationManager NotificationManager { get; set; } = new NotificationManager();

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        NotificationManager.ShowError(error.Message);
    }

    public void OnNext(Exception exception)
    {
        NotificationManager.ShowError(exception.Message);
    }
}
