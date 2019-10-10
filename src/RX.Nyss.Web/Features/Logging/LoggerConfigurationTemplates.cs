namespace RX.Nyss.Web.Features.Logging
{
    public static class LoggerConfigurationTemplates
    {
        public static readonly string TemplateMessagePrefix = "[{Level: u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} ";

        public static readonly string ExceptionTemplate = TemplateMessagePrefix + "{Message:lj}{NewLine}{Exception}";

        public static readonly string CallerTemplate = TemplateMessagePrefix + "[{Caller}] {Message:lj}{NewLine}";

        public static readonly string SourceContextTemplate = TemplateMessagePrefix + "[{SourceContext}] {Message:lj}{NewLine}";
    }
}
