using System.Linq;
using Microsoft.AspNetCore.Authorization;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Authorization
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RolesAttribute(params Role[] roles)
        {
            var stringRoles = roles.Select(r => r.ToString());
            Roles = string.Join(",", stringRoles);
        }
    }
}
