using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Utils
{
    public class NeedsPolicyAttribute : AuthorizeAttribute
    {
        public NeedsPolicyAttribute(Policy policy)
        {
            Policy = policy.ToString();
        }
    }
}
