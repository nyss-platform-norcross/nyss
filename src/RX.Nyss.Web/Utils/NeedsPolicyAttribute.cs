using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Utils
{
    public class NeedsPolicyAttribute : AuthorizeAttribute
    {
        public NeedsPolicyAttribute(AuthenticationPolicy authenticationPolicy)
        {
            Policy = authenticationPolicy.ToString();
        }
    }
}
