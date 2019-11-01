using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public interface IHealthRiskService
    {
        Task<Result<IEnumerable<HealthRiskResponseDto>>> GetHealthRisks(int languageId);
        Task<Result<EditHealthRiskRequestDto>> GetHealthRisk(int id);
        Task<Result<int>> CreateHealthRisk(CreateHealthRiskRequestDto createHealthRiskDto);
        Task<Result> EditHealthRisk(EditHealthRiskRequestDto editHealthRiskDto);
        Task<Result> RemoveHealthRisk(int id);
    }
}
