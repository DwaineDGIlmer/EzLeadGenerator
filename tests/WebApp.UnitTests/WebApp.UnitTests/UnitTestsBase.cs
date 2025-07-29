#nullable disable
using Moq;
using Moq.Protected;
using System.Net;

namespace WebApp.UnitTests;

/// <summary>
/// Provides a base class for unit tests, offering utility methods and configurations to simplify the setup of test
/// environments.
/// </summary>
/// <remarks>This class includes functionality for initializing shared services, creating mock HTTP
/// clients, and generating test log events. It is designed to streamline common tasks in unit testing scenarios,
/// such as mocking HTTP responses or serializing log events.</remarks>
public class UnitTestsBase
{
    /// <summary>
    /// Creates a mock <see cref="HttpClient"/> instance that returns a predefined response.
    /// </summary>
    /// <remarks>The returned <see cref="HttpClient"/> is configured with a base address of
    /// "http://localhost/". This method is useful for testing components that depend on <see cref="HttpClient"/>
    /// without making actual HTTP requests.</remarks>
    /// <param name="responseContent">The content to include in the mock HTTP response.</param>
    /// <param name="statusCode">The HTTP status code to return in the mock response. Defaults to <see cref="HttpStatusCode.OK"/>.</param>
    /// <returns>A mock <see cref="HttpClient"/> configured to return the specified response content and status code.</returns>
    public static HttpClient GetMockHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent),
            })
            .Verifiable();
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }

    /// <summary>
    /// Creates a mock <see cref="HttpClient"/> that throws a specified exception for all HTTP requests.
    /// </summary>
    /// <remarks>The returned <see cref="HttpClient"/> is intended for use in unit tests where
    /// simulating failure scenarios is required. The <see cref="HttpClient.BaseAddress"/> is set to
    /// "http://localhost/" by default.</remarks>
    /// <param name="exceptionToThrow">The <see cref="Exception"/> to be thrown when any HTTP request is made using the returned <see
    /// cref="HttpClient"/>.</param>
    /// <returns>A mock <see cref="HttpClient"/> instance configured to throw the specified exception for all requests.</returns>
    public static HttpClient GetMockHttpClient(Exception exceptionToThrow)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exceptionToThrow)
            .Verifiable();
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }

    /// <summary>
    /// Creates a mock <see cref="HttpClient"/> instance configured to return the specified response message.
    /// </summary>
    /// <remarks>This method is useful for testing scenarios where a controlled response from an <see
    /// cref="HttpClient"/> is required. The returned <see cref="HttpClient"/> uses a mocked <see
    /// cref="HttpMessageHandler"/> to intercept requests and provide the specified response.</remarks>
    /// <param name="responseMessage">The <see cref="HttpResponseMessage"/> to be returned by the mock <see cref="HttpClient"/> for any request.</param>
    /// <returns>A mock <see cref="HttpClient"/> instance that returns the specified <paramref name="responseMessage"/> for
    /// all requests.</returns>
    public static HttpClient CreateMockHttpClient(HttpResponseMessage responseMessage, string baseAddress = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(baseAddress ?? "http://example.com")
        };
        return client;
    }

    /// <summary>
    /// Creates a mock implementation of <see cref="IHttpClientFactory"/> that returns the specified <see
    /// cref="HttpClient"/>  for the given client name.
    /// </summary>
    /// <remarks>This method is useful for testing scenarios where a specific <see cref="HttpClient"/>
    /// instance needs to be injected  into code that depends on <see cref="IHttpClientFactory"/>.</remarks>
    /// <param name="httpClient">The <see cref="HttpClient"/> instance to be returned by the mock factory.</param>
    /// <param name="clientName">The name of the client for which the <see cref="HttpClient"/> will be returned.</param>
    /// <returns>A mock implementation of <see cref="IHttpClientFactory"/> that returns the specified <see
    /// cref="HttpClient"/>  when <see cref="IHttpClientFactory.CreateClient(string)"/> is called with the given
    /// client name.</returns>
    public static IHttpClientFactory CreateMockHttpClientFactory(HttpClient httpClient, string clientName)
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient(clientName)).Returns(httpClient);
        return mockFactory.Object;
    }
}
