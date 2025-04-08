using System.Collections;

namespace Bridge.Domain.Exceptions;

public class InternalServerErrorException(string? message = null, IDictionary? data = null) : HttpStatusException(message, data)
{
    public override int StatusCode => 500;
}