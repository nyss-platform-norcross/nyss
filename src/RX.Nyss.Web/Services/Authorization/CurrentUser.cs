using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Services.Authorization
{
    public class CurrentUser
    {
        public string Name { get; set; }

        public IEnumerable<Role> Roles { get; set; }
    }
}
