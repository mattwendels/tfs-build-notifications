using System;
using System.IO;
using Tfs.BuildNotifications.Common.Telemetry.Interfaces;

namespace Tfs.BuildNotifications.Common.Telemetry
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogService : ILogService
    {
        private string _logFilePath = "tfs.buildnotifications.log";

        private static object _logFileLock = new object();

        public void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            LogToFile(message, logLevel);
        }

        public void Log(Exception e, LogLevel logLevel = LogLevel.Error)
        {
            LogToFile(e.ToString(), logLevel);
        }

        public void Log(string message, Exception e, LogLevel logLevel = LogLevel.Error)
        {
            LogToFile($"{message} - {e.ToString()}", logLevel);
        }

        private void LogToFile(string message, LogLevel logLevel)
        {
            try
            {
                lock(_logFileLock)
                {
                    using (var writer = new StreamWriter(_logFilePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now} - {logLevel.ToString().ToUpper()} | {message}");
                    }
                }
            }
            catch { }
        }
    }
}
