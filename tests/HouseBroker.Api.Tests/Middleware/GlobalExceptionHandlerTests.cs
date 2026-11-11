using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HouseBroker.Api.Middleware;
using HouseBroker.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HouseBroker.Api.Tests.Middleware;

/// <summary>Checks that service exceptions become the right status code and ProblemDetails body.</summary>
public class GlobalExceptionHandlerTests
{
    private static (GlobalExceptionHandler Handler, DefaultHttpContext Context) Create()
    {
        // Real MVC ProblemDetailsFactory + IProblemDetailsService, as registered in Program.cs.
        var services = new ServiceCollection().AddLogging();
        services.AddControllers();
        services.AddProblemDetails();
        var provider = services.BuildServiceProvider();

        var handler = new GlobalExceptionHandler(
            provider.GetRequiredService<ProblemDetailsFactory>(),
            provider.GetRequiredService<IProblemDetailsService>(),
            NullLogger<GlobalExceptionHandler>.Instance);

        var ctx = new DefaultHttpContext { RequestServices = provider };
        ctx.Request.Headers.Accept = "application/json";
        ctx.Response.Body = new MemoryStream();
        return (handler, ctx);
    }

    private static async Task<JsonElement> ReadBodyAsync(HttpContext ctx)
    {
        ctx.Response.Body.Position = 0;
        return (await JsonDocument.ParseAsync(ctx.Response.Body)).RootElement;
    }

    public static TheoryData<Exception, int> Cases => new()
    {
        { new ValidationException("Price must be greater than zero."), StatusCodes.Status400BadRequest },
        { new InvalidCredentialsException(), StatusCodes.Status401Unauthorized },
        { new UnauthorizedAccessException("not yours"), StatusCodes.Status403Forbidden },
        { new KeyNotFoundException("missing"), StatusCodes.Status404NotFound },
        { new ConflictException("taken"), StatusCodes.Status409Conflict },
        { new InvalidOperationException("boom"), StatusCodes.Status500InternalServerError }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task TryHandleAsync_Exception_MapsToStatusCode(Exception ex, int expectedStatus)
    {
        var (handler, ctx) = Create();

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(expectedStatus);
        var body = await ReadBodyAsync(ctx);
        body.GetProperty("status").GetInt32().Should().Be(expectedStatus);
        body.TryGetProperty("traceId", out _).Should().BeTrue("ProblemDetailsFactory adds the trace id");
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_ReturnsFieldErrors()
    {
        var (handler, ctx) = Create();
        var ex = new ValidationException([new ValidationFailure("Price", "Price must be greater than 0.")]);

        await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        var body = await ReadBodyAsync(ctx);
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        body.GetProperty("errors").GetProperty("Price")[0].GetString().Should().Be("Price must be greater than 0.");
    }

    [Fact]
    public async Task TryHandleAsync_ServerError_DoesNotLeakMessage()
    {
        var (handler, ctx) = Create();

        await handler.TryHandleAsync(ctx, new InvalidOperationException("secret detail"), CancellationToken.None);

        var body = await ReadBodyAsync(ctx);
        body.ToString().Should().NotContain("secret detail");
    }
}
