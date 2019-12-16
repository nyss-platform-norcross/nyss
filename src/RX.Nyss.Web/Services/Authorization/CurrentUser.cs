using System.Collections.Generic;

namespace RX.Nyss.Web.Services.Authorization
{
    public class CurrentUser
    {
        public string Name { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
