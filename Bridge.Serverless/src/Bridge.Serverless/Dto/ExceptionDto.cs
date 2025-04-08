using System.Collections;

namespace Bridge.Serverless.Dto;

public class ExceptionDto
{
    public required string Title { get; set; }
    public required int Status { get; set; }
    public required string Path { get; set; }
    public string? StackTrace { get; set; }
    public IDictionary? Data { get; set; }
}