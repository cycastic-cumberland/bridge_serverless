using Bridge.Domain.Exceptions;
using Bridge.Serverless.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bridge.Serverless.Services;

public class ApiExceptionFilter : IAsyncExceptionFilter
{
    private readonly IWebHostEnvironment env;
    
    public ApiExceptionFilter(IWebHostEnvironment env)
    {
        this.env = env;
    }

    private string? GetStacktrace(Exception e) => env.IsDevelopment() ? e.StackTrace : null;

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        ExceptionDto result;
        switch (exception)
        {
            case HttpStatusException e:
            {
                result = new()
                {
                    Title = e.Message,
                    Status = e.StatusCode,
                    Path = context.HttpContext.Request.Path,
                    StackTrace = GetStacktrace(exception),
                    Data = e.CustomData
                };
                break;
            }
            default:
            {
                result = new()
                {
                    Title = exception.Message,
                    Status = 500,
                    Path = context.HttpContext.Request.Path,
                    StackTrace = GetStacktrace(exception),
                    Data = exception.Data
                };
                break;
            }
        }

        context.Result = new JsonResult(result)
        {
            StatusCode = result.Status
        };
        return Task.CompletedTask;
    }
}