using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Utils.AuthorizationHandler
{
    public abstract class ClaimWithValueAuthorizationHandler<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
    {
        private readonly ClaimType _claimType;
        private readonly string _claimValue;

        protected ClaimWithValueAuthorizationHandler(ClaimType claimType, string claimValue)
        {
            _claimType = claimType;
            _claimValue = claimValue;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
        {
            var hasClaimWithValue = context.User.Claims.Any(c => c.Type == _claimType.ToString() && c.Value == _claimValue);
            if (hasClaimWithValue)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
