using System;

namespace FolderSync
{
    /// <summary>
    /// logging of the operation type
    /// </summary>
    public enum LogType
    {
        Creation,
        Copy,
        Removal
    }

    public interface ILogger
    {
        LogType logType { get; set; }

        /// <summary>
        /// Event to be fired when logging message
        /// </summary>
        event Action<LogType, string, string> onLog;

        /// <summary>
        /// Logging message
        /// </summary>
        /// <param name="logType">operation type</param>
        /// <param name="message">message to be written</param>
        void LogMessage(LogType logType, string message);

    }
}
