#nullable disable
using Application.Contracts;
using Loggers.Contracts;
using Loggers.Publishers;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Provides a mock implementation of the <see cref="ISearch"/> interface for testing purposes.
    /// </summary>
    /// <remarks>This class is designed to simulate search functionality by returning a predefined result of
    /// type <typeparamref name="T"/>. It is useful for unit testing scenarios where a real search implementation is not
    /// required or available.</remarks>
    /// <typeparam name="T">The type of the search result returned by the mock implementation.</typeparam>
    public sealed class MockSearch<P>(P result) : ISearch
    {
        private readonly P _result = result;

        public Task<IEnumerable<T>> FetchOrganicResults<T>(string query, string location)
        {
            return Task.FromResult((IEnumerable<T>)_result);
        }
        public Task<IEnumerable<T>> FetchSearchResults<T>(string query, string location)
        {
            return Task.FromResult((IEnumerable<T>)_result);
        }
    }

    /// <summary>
    /// A mock implementation of the <see cref="ILogger"/> interface for testing purposes.
    /// </summary>
    /// <remarks>This logger captures log messages in an in-memory collection and publishes them using the
    /// provided <see cref="IPublisher"/>. It can be used to verify logging behavior in unit tests by inspecting the
    /// captured messages.</remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MockLogger"/> class with the specified log level and publisher.
    /// </remarks>
    /// <param name="logLevel">The minimum <see cref="LogLevel"/> at which log messages will be processed.</param>
    /// <param name="publisher">The <see cref="IPublisher"/> instance used to publish log messages. Cannot be <see langword="null"/>.</param>
    public sealed class MockLogger<T>(LogLevel logLevel, IPublisher publisher = null) : ILogger<T>
    {
        private readonly LogLevel _logLevel = logLevel;
        private readonly IPublisher _publisher = publisher ?? new ConsolePublisher(20);

        /// <summary>
        /// Gets the collection of messages.
        /// </summary>
        public IList<string> Messages { get; } = [];

        /// <summary>
        /// Determines whether the collection contains any message that includes the specified substring.
        /// </summary>
        /// <param name="message">The substring to search for within the messages. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if at least one message in the collection contains the specified substring;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Contains(string message) => Messages.Any(m => m.Contains(message));

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <remarks>The scope can be used to group a set of operations together, providing contextual
        /// information that can be accessed during the lifetime of the scope. This is commonly used in logging
        /// frameworks to include additional context in log messages.</remarks>
        /// <typeparam name="TState">The type of the state to associate with the scope.</typeparam>
        /// <param name="state">The state to associate with the scope. This can be used to provide contextual information.</param>
        /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope when disposed.</returns>
        public IDisposable BeginScope<TState>(TState state) => null!;

        /// <summary>
        /// Determines whether logging is enabled for the specified log level.
        /// </summary>
        /// <param name="logLevel">The log level to check.</param>
        /// <returns><see langword="true"/> if logging is enabled for the specified <paramref name="logLevel"/>;  otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

        /// <summary>
        /// Logs a formatted message at the specified log level.
        /// </summary>
        /// <remarks>The method logs the message only if the specified <paramref name="logLevel"/> is
        /// enabled. The formatted message is added to the internal message collection and written to the
        /// publisher.</remarks>
        /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
        /// <param name="logLevel">The severity level of the log message.</param>
        /// <param name="eventId">The identifier for the event being logged.</param>
        /// <param name="state">The state object containing information to be logged.</param>
        /// <param name="exception">The exception related to the log entry, or <see langword="null"/> if no exception is associated.</param>
        /// <param name="formatter">A function that formats the <paramref name="state"/> and <paramref name="exception"/> into a log message
        /// string.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var message = $"[{logLevel}] {formatter(state, exception)}";
                Messages.Add(message);
                _publisher.WriteLine(message);
            }
        }
    }

}
