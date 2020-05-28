using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Utils
{
    public abstract class ResourceAccessHandler<THandler> : AuthorizationHandler<ResourceAccessHandler<THandler>.Requirement>
        where THandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _routeParameterName;

        protected ResourceAccessHandler(string routeParameterName, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _routeParameterName = routeParameterName;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirement requirement)
        {
            var entityId = _httpContextAccessor.GetResourceParameter(_routeParameterName);

            if (entityId.HasValue && await HasAccess(entityId.Value, requirement.ReadOnly))
            {
                context.Succeed(requirement);
            }
        }

        protected abstract Task<bool> HasAccess(int entityId, bool readOnly);

        public class Requirement : IAuthorizationRequirement
        {
            public bool ReadOnly { get; }

            public Requirement(bool readOnly = false)
            {
                ReadOnly = readOnly;
            }
        }
    }
}
