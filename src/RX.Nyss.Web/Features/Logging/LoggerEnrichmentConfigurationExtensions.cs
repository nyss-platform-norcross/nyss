using Serilog.Configuration;

namespace RX.Nyss.Web.Features.Logging
{
    public static class LoggerEnrichmentConfigurationExtensions
    {
        public static Serilog.LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration) => enrichmentConfiguration.With<CallerLogEventEnricher>();
    }
}
