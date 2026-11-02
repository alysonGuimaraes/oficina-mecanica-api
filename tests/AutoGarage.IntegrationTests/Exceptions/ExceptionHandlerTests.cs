using AutoGarage.Api.Exceptions;
using AutoGarage.Domain.Exceptions.Customers.Exceptions;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace AutoGarage.IntegrationTests.Exceptions;

public class ExceptionHandlerTests
{
    [Theory]
    [InlineData("https", "oficina.example.com", "https://oficina.example.com/scalar#")]
    [InlineData("http", "localhost:7169", "http://localhost:7169/scalar#")]
    public async Task TryHandleAsync_DomainError_ShouldBuildTypeFromRequestSchemeAndHost(string scheme, string host, string expectedBase)
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var sut = new ExceptionHandler(problemDetailsService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.Path = "/api/customers/1";
        var exception = new CustomerNotFoundException(Guid.NewGuid());

        var handled = await sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.ShouldBeTrue();
        problemDetailsService.Context!.ProblemDetails.Type.ShouldBe($"{expectedBase}{exception.ErrorType}");
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Context { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.FromResult(true);
        }
    }
}
