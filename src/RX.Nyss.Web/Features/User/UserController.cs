using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authorization;
using RX.Nyss.Web.Features.Base;
using RX.Nyss.Web.Features.DataContract;
using RX.Nyss.Web.Features.User.Dto;

namespace RX.Nyss.Web.Features.User
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("RegisterGlobalCoordinator")]
        [Roles(Role.SystemAdministrator)]
        public async Task<Result> RegisterGlobalCoordinator([FromBody]GlobalCoordinatorInDto globalCoordinatorInDto)
        {
            return await _userService.RegisterGlobalCoordinator(globalCoordinatorInDto);
        }
    }
}
