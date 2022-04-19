using System.Runtime.Serialization;

namespace DAL.Exceptions;

public class DataverseConnectionException: Exception
{
    public DataverseConnectionException() { }

    protected DataverseConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public DataverseConnectionException(string message) : base(message) { }
}