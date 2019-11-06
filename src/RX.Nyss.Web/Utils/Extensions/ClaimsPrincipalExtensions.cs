using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static IEnumerable<string> GetRoles(this ClaimsPrincipal claimsPrincipal) => 
            claimsPrincipal.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
    }
}
