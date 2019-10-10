using System.Diagnostics;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public class CallerLogEventEnricher : ILogEventEnricher
    {
        private const int SerilogNestLevel = 5;
        public static readonly string CallerPropertyName = "Caller";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Get Serilog frame
            var stack = new StackFrame(SerilogNestLevel);
            var method = stack.GetMethod();
            if (logEvent.Properties.ContainsKey(CallerPropertyName) || !stack.HasMethod() || !IsSerilogAdapter(method))
            {
                return;
            }

            // Get Caller and put it to Log Context
            stack = new StackFrame(SerilogNestLevel + 1);
            method = stack.GetMethod();
            var caller = $"{method.DeclaringType.FullName}.{method.Name}";
            logEvent.AddPropertyIfAbsent(new LogEventProperty(CallerPropertyName, new ScalarValue(caller)));
        }

        private static bool IsSerilogAdapter(MemberInfo method)
        {
            return method.DeclaringType != null && method.DeclaringType == typeof(SerilogLoggerAdapter);
        }
    }
}
