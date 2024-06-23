using System.Runtime.InteropServices;

namespace AvaMSN.Utils.Notifications;

/// <summary>
/// Contains macOS notification functions imported from native code.
/// </summary>
public static partial class MacOsNotifications
{
    [LibraryImport("libNotifications.dylib")]
    public static partial void requestAuthorization();

    [LibraryImport("libNotifications.dylib", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    public static partial void showNotification(string title, string body);
}