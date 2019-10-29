using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public interface IHealthRiskService
    {
        Task<IEnumerable<HealthRiskDto>> GetHealthRisks();
    }
}
