using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.User
{
    [Route("api")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Lists all users on the national society level.
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society to list the users from</param>
        /// <returns></returns>
        [HttpGet("nationalSociety/{nationalSocietyId:int}/user/list")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> List(int nationalSocietyId) =>
            await _userService.GetUsersInNationalSociety(nationalSocietyId);

        /// <summary>
        /// Gets basic data about the user
        /// </summary>
        /// <param name="nationalSocietyUserId">User Id</param>
        /// <returns></returns>
        [HttpGet("nationalSociety/user/{nationalSocietyUserId:int}/basicData")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor)]
        public async Task<Result> GetBasicData(int nationalSocietyUserId) =>
            await _userService.GetBasicData(nationalSocietyUserId);
    }
}

