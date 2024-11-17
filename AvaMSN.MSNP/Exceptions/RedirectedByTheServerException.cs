namespace AvaMSN.MSNP.Exceptions;

public class RedirectedByTheServerException : Exception
{
    public string Server { get; } = string.Empty;
    public int Port { get; }

    public RedirectedByTheServerException() { }

    public RedirectedByTheServerException(string message) : base(message) { }

    public RedirectedByTheServerException(string message, Exception innerException) : base(message, innerException) { }

    public RedirectedByTheServerException(string message, string server, int port) : this(message)
    {
        Server = server;
        Port = port;
    }
}
