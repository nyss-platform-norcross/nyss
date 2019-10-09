using System.Collections.Generic;
using System.Threading.Tasks;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public interface IHealthRiskService
    {
        Task<IEnumerable<HealthRiskDto>> GetHealthRisks();
    }
}