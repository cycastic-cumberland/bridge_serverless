using System.Collections;

namespace Bridge.Domain.Exceptions;

public abstract class HttpStatusException(string? message = null, IDictionary? data = null) : Exception(message)
{
    public abstract int StatusCode { get; }

    public IDictionary? CustomData => data;
}