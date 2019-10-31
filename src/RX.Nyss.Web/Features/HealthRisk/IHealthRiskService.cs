using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public interface IHealthRiskService
    {
        Task<IEnumerable<HealthRiskResponseDto>> GetHealthRisks();

        Task<Result<int>> CreateHealthRisk(CreateHealthRiskRequestDto createHealthRiskDto);
        Task<Result> EditHealthRisk(EditHealthRiskRequestDto editHealthRiskDto);
    }
}
