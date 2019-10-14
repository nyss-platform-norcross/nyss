using System.Linq;
using Microsoft.AspNetCore.Authorization;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Utils
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
