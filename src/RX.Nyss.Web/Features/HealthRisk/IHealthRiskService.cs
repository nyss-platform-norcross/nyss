using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public interface IHealthRiskService
    {
        Task<IEnumerable<HealthRiskResponseDto>> GetHealthRisks(int languageId);
        Task<EditHealthRiskRequestDto> GetHealthRisk(int id);
        Task<Result<int>> CreateHealthRisk(CreateHealthRiskRequestDto createHealthRiskDto);
        Task<Result> EditHealthRisk(EditHealthRiskRequestDto editHealthRiskDto);
        Task<Result> RemoveHealthRisk(int id);
    }
}
