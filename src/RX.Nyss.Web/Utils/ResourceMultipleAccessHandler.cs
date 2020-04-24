using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RX.Nyss.Web.Utils
{
    public abstract class ResourceMultipleAccessHandler<THandler> : AuthorizationHandler<ResourceMultipleAccessHandler<THandler>.Requirement>
        where THandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _jsonPropertyName;

        protected ResourceMultipleAccessHandler(string jsonPropertyName, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _jsonPropertyName = jsonPropertyName;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirement requirement)
        {
            _httpContextAccessor.HttpContext.Request.EnableBuffering();

            var jsonDocument = await JsonDocument.ParseAsync(_httpContextAccessor.HttpContext.Request.Body);

            var ids = jsonDocument.RootElement.GetProperty(_jsonPropertyName).EnumerateArray().Select(a => a.GetInt32()).ToList();

            _httpContextAccessor.HttpContext.Request.Body.Position = 0;

            if (await HasAccess(ids))
            {
                context.Succeed(requirement);
            }
        }

        protected abstract Task<bool> HasAccess(IEnumerable<int> entityIds);

        public class Requirement : IAuthorizationRequirement
        {
        }
    }
}
