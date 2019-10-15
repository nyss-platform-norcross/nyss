using System.Threading.Tasks;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<Result> RegisterGlobalCoordinator(GlobalCoordinatorInDto globalCoordinatorInDto);
    }
}
