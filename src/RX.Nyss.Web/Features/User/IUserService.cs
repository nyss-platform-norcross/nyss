using System.Threading.Tasks;
using RX.Nyss.Web.Features.DataContract;
using RX.Nyss.Web.Features.User.Dto;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<Result> RegisterGlobalCoordinator(GlobalCoordinatorInDto globalCoordinatorInDto);
    }
}
