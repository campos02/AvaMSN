namespace AvaMSN.MSNP.Exceptions;

public class MsnpServerAuthException : Exception
{
    public MsnpServerAuthException() { }

    public MsnpServerAuthException(string message) : base(message) { }

    public MsnpServerAuthException(string message, Exception innerException) : base(message, innerException) { }
}
