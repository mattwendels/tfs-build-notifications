using System;

namespace Tfs.BuildNotifications.Common.Telemetry.Interfaces
{
    public interface ILogService
    {
        void Log(string message, LogLevel logLevel = LogLevel.Info);
        void Log(Exception e, LogLevel logLevel = LogLevel.Error);
        void Log(string message, Exception e, LogLevel logLevel = LogLevel.Error);
    }
}
