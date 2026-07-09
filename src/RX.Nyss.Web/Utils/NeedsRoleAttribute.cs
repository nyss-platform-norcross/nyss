using System.Linq;
using Microsoft.AspNetCore.Authorization;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Utils
{
    public class NeedsRoleAttribute : AuthorizeAttribute
    {
        public NeedsRoleAttribute(params Role[] roles)
        {
            var stringRoles = roles.Select(r => r.ToString());
            Roles = string.Join(",", stringRoles);
        }
    }
}
