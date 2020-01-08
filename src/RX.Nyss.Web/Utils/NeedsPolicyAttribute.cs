using Microsoft.AspNetCore.Authorization;
using RX.Nyss.Web.Features.Common;

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
