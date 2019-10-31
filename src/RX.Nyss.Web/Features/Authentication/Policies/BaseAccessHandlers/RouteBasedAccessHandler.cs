using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RX.Nyss.Web.Features.Authentication.Policies.BaseAccessHandlers
{
    public abstract class RouteBasedAccessHandler<T> : AuthorizationHandler<T> where T: IAuthorizationRequirement
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _routeParameterName;

        protected RouteBasedAccessHandler(IHttpContextAccessor httpContextAccessor, string routeParameterName)
        {
            _httpContextAccessor = httpContextAccessor;
            _routeParameterName = routeParameterName;
        }

        public int? GetResourceIdFromRoute()
        {
            var routeValue = _httpContextAccessor.HttpContext.Request.RouteValues[_routeParameterName];
            return int.TryParse(routeValue.ToString(), out var id)
                ? (int?)id
                : null;
        }
    }
}
