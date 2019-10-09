using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Features.Authorization
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RolesAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
