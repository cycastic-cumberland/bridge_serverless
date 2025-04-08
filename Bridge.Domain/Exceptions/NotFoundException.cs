using System.Collections;

namespace Bridge.Domain.Exceptions;

public class NotFoundException(string? message = null, IDictionary? data = null) : HttpStatusException(message, data)
{
    public override int StatusCode => 404;
}