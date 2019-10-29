using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Utils.AuthorizationHandler
{
    public class IsDataOwnerRequirement : IAuthorizationRequirement
    {
    }

    public class IsDataOwnerHandler : ClaimWithValueAuthorizationHandler<IsDataOwnerRequirement>
    {
        public IsDataOwnerHandler() 
            : base(ClaimType.IsDataOwner, bool.TrueString)
        {
        }
    }
}
