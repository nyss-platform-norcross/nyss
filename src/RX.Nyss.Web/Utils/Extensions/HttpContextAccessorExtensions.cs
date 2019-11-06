using Microsoft.AspNetCore.Http;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class HttpContextAccessorExtensions
    {
        public static int? GetRouteParameterAsInt(this IHttpContextAccessor httpContextAccessor, string routeParameterName)
        {
            var routeValue = httpContextAccessor.HttpContext.Request.RouteValues[routeParameterName];
            return int.TryParse(routeValue.ToString(), out var id)
                ? (int?)id
                : null;
        }
    }
}
