using System.Threading.Tasks;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Manager.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Manager
{
    public interface IManagerService
    {
        Task<Result> CreateManager(int nationalSocietyId, CreateManagerRequestDto createManagerRequestDto);
        Task<Result<GetManagerResponseDto>> GetManager(int managerId);
        Task<Result> UpdateManager(int managerId, EditManagerRequestDto editManagerRequestDto); 
        Task<Result> DeleteManager(int managerId);
    }
}

