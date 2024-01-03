using System;
using System.IO;

namespace FolderSync
{
    /// <summary>
    /// logger for console
    /// </summary>
    //public class ConsoleLogger : ILogger
    //{
    //    #region public property
    //    public LogType logType { get; set; }
    //    #endregion

    //    #region event
    //    public event Action<LogType, string> onLog = (type, message) => { };
    //    #endregion

    //    /// <summary>
    //    /// logging to console
    //    /// </summary>
    //    /// <param name="logType">operation type</param>
    //    /// <param name="message">message to show</param>
    //    public void LogMessage(LogType logType, string message)
    //    {
    //        onLog(logType, message);
    //        //Console.WriteLine($"Logging on console: '{message}'");
    //    }
    //}

    public class FileLogger : ILogger
    {
        #region public property
        public LogType logType { get; set; }
        #endregion

        #region private property

        private readonly string _filePath;

        #endregion

        #region event
        public event Action<LogType, string> onLog = (logType, message) => {};
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor +1 
        /// </summary>
        /// <param name="filePath">file path string</param>
        public FileLogger(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be empty or null.", nameof(filePath));
            }
            
            _filePath = filePath;
            var directory = !string.IsNullOrWhiteSpace(Path.GetDirectoryName(_filePath)) ? Path.GetDirectoryName(_filePath): _filePath;
            
            Console.WriteLine(Path.GetDirectoryName(_filePath));

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                throw new ArgumentException("Directory path derived from the file path is empty or null.", nameof(filePath));
            }


        }

        #endregion

        /// <summary>
        /// Writing to file
        /// </summary>
        /// <param name="logType">operation type</param>
        /// <param name="message">message to write</param>
        public void LogMessage(LogType logType, string message)
        {
            Console.WriteLine($"[{DateTime.Now}]: {logType} -> {message}");

            // writing at the end of the file
            using (var fileStr = new StreamWriter(File.OpenWrite(Path.Combine(_filePath, "sync_log.txt"))))
            {
                fileStr.BaseStream.Seek(0, SeekOrigin.End);
                fileStr.WriteLine($"[{DateTime.Now}]: {logType} -> {message}");
            }
            Console.WriteLine("Logged on file!");

        }
    }

}

