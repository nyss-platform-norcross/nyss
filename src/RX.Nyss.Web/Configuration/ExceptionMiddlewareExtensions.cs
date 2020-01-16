using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Configuration
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app) =>
            app.UseExceptionHandler(appError => appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (contextFeature != null)
                {
                    var logger = context.RequestServices.GetService(typeof(ILoggerAdapter)) as ILoggerAdapter;
                    logger?.Error(contextFeature.Error, "There was an unhandled exception.");

                    var result = Error(GetErrorKey(contextFeature.Error));

                    var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await context.Response.WriteAsync(resultJson);
                }
            }));

        private static string GetErrorKey(Exception exception) =>
            exception switch
            {
                DbUpdateException dbUpdateException when dbUpdateException.InnerException is SqlException sqlException =>
                    GetDbUpdateExceptionErrorKey(sqlException),

                _ =>
                    ResultKey.UnexpectedError
            };
        
        private static string GetDbUpdateExceptionErrorKey(SqlException sqlException) =>
            sqlException.Number switch
            {
                547 => ResultKey.SqlExceptions.ForeignKeyViolation,
                2627 => ResultKey.SqlExceptions.DuplicatedValue,
                2601 => ResultKey.SqlExceptions.DuplicatedValue,
                _ => ResultKey.SqlExceptions.GeneralError
            };
    }
}
