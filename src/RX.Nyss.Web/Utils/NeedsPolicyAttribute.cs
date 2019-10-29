using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Utils
{
    public class NeedsPolicyAttribute : AuthorizeAttribute
    {
        public NeedsPolicyAttribute(NyssAuthorizationPolicy policy)
        {
            Policy = policy.ToString();
        }
    }
}
