namespace Application.Enums
{
    /// <summary>
    /// Defines a set of event codes used for logging and telemetry purposes.
    /// </summary>
    /// <remarks>The <see cref="LogEvents"/> enumeration provides a standardized set of event identifiers for
    /// categorizing log messages and telemetry data. These event codes are typically used to distinguish between
    /// different types of log entries, such as exceptions, warnings, informational messages, and specific application
    /// events (e.g., cache operations or user actions). <para> Each value in the enumeration is associated with a
    /// unique integer code, which can be used to filter, query, or analyze log data in logging frameworks or telemetry
    /// systems. </para></remarks>
    public enum LogEvents
    {
        /// <summary>
        /// Represents the absence of a specific value or state.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents an exception event type with a unique identifier.
        /// </summary>
        /// <remarks>This value is typically used to categorize or identify exception-related events in
        /// logging or telemetry systems.</remarks>
        Exception = 1001,

        /// <summary>
        /// Represents a warning message type with a specific code.
        /// </summary>
        /// <remarks>This value is typically used to indicate non-critical issues that require attention
        /// but do not prevent the application from continuing execution.</remarks>
        Warning = 1002,

        /// <summary>
        /// Represents an informational log level.
        /// </summary>
        /// <remarks>This log level is typically used to log informational messages that highlight the
        /// progress of the application at a coarse-grained level.</remarks>
        Information = 1003,

        /// <summary>
        /// Represents the event ID for a cache hit.
        /// </summary>
        /// <remarks>This event ID is typically used to log or track occurrences where a requested item is
        /// found in the cache.</remarks>
        CacheHit = 2001,

        /// <summary>
        /// Represents the status code for a cache miss.
        /// </summary>
        /// <remarks>This status code indicates that the requested item was not found in the
        /// cache.</remarks>
        CacheMiss = 2002,

        /// <summary>
        /// Represents the event code for a user login action.
        /// </summary>
        /// <remarks>This code is typically used to identify and log user login events in the
        /// system.</remarks>
        UserLogin = 3001,

        /// <summary>
        /// Represents the event code for a user logout action.
        /// </summary>
        /// <remarks>This code is typically used to identify and handle user logout events in the
        /// system.</remarks>
        UserLogout = 3002
    }
}
