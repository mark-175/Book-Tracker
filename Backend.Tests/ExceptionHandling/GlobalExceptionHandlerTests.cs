using BookTracker.Api.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookTracker.Api.Tests.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ProblemDetailsServiceReturnsTrue_SetsResponseStatusCodeTo500()
    {
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(true);
        var handler = new GlobalExceptionHandler(Mock.Of<ILogger<GlobalExceptionHandler>>(), problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/books";
        var exception = new InvalidOperationException("boom");

        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ProblemDetailsServiceReturnsFalse_StillSetsResponseStatusCodeTo500()
    {
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(false);
        var handler = new GlobalExceptionHandler(Mock.Of<ILogger<GlobalExceptionHandler>>(), problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/books";
        var exception = new InvalidOperationException("boom");

        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_CallsTryWriteAsyncExactlyOnceWithExpectedProblemDetailsContext()
    {
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        ProblemDetailsContext? capturedContext = null;
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
            .ReturnsAsync(true);
        var handler = new GlobalExceptionHandler(Mock.Of<ILogger<GlobalExceptionHandler>>(), problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/books";
        var exception = new InvalidOperationException("boom");

        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        problemDetailsServiceMock.Verify(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
        Assert.NotNull(capturedContext);
        Assert.Same(httpContext, capturedContext!.HttpContext);
        Assert.Same(exception, capturedContext.Exception);
        Assert.Equal(StatusCodes.Status500InternalServerError, capturedContext.ProblemDetails.Status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TryHandleAsync_ReturnsWhateverTryWriteAsyncResolvesTo(bool tryWriteResult)
    {
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(tryWriteResult);
        var handler = new GlobalExceptionHandler(Mock.Of<ILogger<GlobalExceptionHandler>>(), problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/books";
        var exception = new InvalidOperationException("boom");

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.Equal(tryWriteResult, handled);
    }
}
