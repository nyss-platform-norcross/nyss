using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Utils.AuthorizationHandler
{
    public abstract class ClaimWithValueAuthorizationHandler<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
    {
        private readonly ClaimType _claimType;
        private readonly string _requiredClaimValue;

        protected ClaimWithValueAuthorizationHandler(ClaimType claimType, string requiredClaimValue)
        {
            _claimType = claimType;
            _requiredClaimValue = requiredClaimValue;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
        {
            var hasClaimWithValue = context.User.Claims.Any(c => c.Type == _claimType.ToString() && c.Value == _requiredClaimValue);
            if (hasClaimWithValue)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
