using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.User
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Register a global coordinator user
        /// </summary>
        /// <param name="globalCoordinatorInDto"></param>
        /// <returns></returns>
        [HttpPost("RegisterGlobalCoordinator")]
        [Roles(Role.Administrator)]
        public async Task<Result> RegisterGlobalCoordinator([FromBody]GlobalCoordinatorInDto globalCoordinatorInDto)
        {
            return await _userService.RegisterGlobalCoordinator(globalCoordinatorInDto);
        }
    }
}
