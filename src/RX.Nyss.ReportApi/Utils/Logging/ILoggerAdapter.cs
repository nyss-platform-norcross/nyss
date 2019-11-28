using System;

namespace RX.Nyss.ReportApi.Utils.Logging
{
    public interface ILoggerAdapter
    {
        void Debug(object obj);

        void DebugFormat(string format, params object[] args);

        void Info(object obj);

        void InfoFormat(string format, params object[] args);

        void Warn(object obj);

        void WarnFormat(string format, params object[] args);

        void Error(object obj);
        
        void Error(Exception exception, string message);

        void ErrorFormat(string format, params object[] args);
    }
}
