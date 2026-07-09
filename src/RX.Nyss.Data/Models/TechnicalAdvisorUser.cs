using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class TechnicalAdvisorUser : User
    {
        public virtual ICollection<TechnicalAdvisorUserGatewayModem>? TechnicalAdvisorUserGatewayModems { get; set; }
    }
}
