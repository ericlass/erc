using System;
using System.Text;

namespace erc
{
    public class SimpleLogger
    {
        private StringBuilder _logBuilder = new();
        public LogLevel LogLevel { get; set; } = LogLevel.Debug;

        public SimpleLogger()
        {
        }

        public SimpleLogger(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        public override string ToString()
        {
            return _logBuilder.ToString();
        }

        public void Log(LogLevel level, string message)
        {
            if (level > LogLevel)
                return;

            //TODO: Add trailing spaces to level, if required
            _logBuilder.Append(level.ToString().ToUpper());
            _logBuilder.Append(" | ");
            _logBuilder.AppendLine(message);
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void Warn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Trace(string message)
        {
            Log(LogLevel.Trace, message);
        }

    }

}
