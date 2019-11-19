using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public static class ContextAccessorExtensions
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
