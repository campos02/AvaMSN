using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaMSN.MSNP.Exceptions;

public class PayloadException : Exception
{
    public PayloadException() { }

    public PayloadException(string message) : base(message) { }

    public PayloadException(string message, Exception innerException) : base(message, innerException) { }
}
