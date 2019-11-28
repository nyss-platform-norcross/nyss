using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Configuration
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app) =>
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var logger = context.RequestServices.GetService(typeof(ILoggerAdapter)) as ILoggerAdapter;
                        logger?.Error(contextFeature.Error, "There was an unhandled exception.");

                        var result = new object();

                        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        await context.Response.WriteAsync(json);
                    }
                });
            });
    }
}
