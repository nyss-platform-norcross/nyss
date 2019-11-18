using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

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
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> List(int nationalSocietyId) =>
            await _userService.GetUsersInNationalSociety(nationalSocietyId, User.GetRoles());

        /// <summary>
        /// Gets basic data about the user
        /// </summary>
        /// <param name="nationalSocietyUserId">User Id</param>
        /// <returns></returns>
        [HttpGet("nationalSociety/user/{nationalSocietyUserId:int}/basicData")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor)]
        public async Task<Result> GetBasicData(int nationalSocietyUserId) =>
            await _userService.GetBasicData(nationalSocietyUserId);

        /// <summary>
        /// Adds an existing technical advisor or a data consumer user to a national society.
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society</param>
        /// <param name="existingUser">The data of the existing user to be added</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/{nationalSocietyId:int}/user/addExisting")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> AddExisting(int nationalSocietyId, AddExistingUserToNationalSocietyRequestDto existingUser) =>
            await _userService.AddExisting(nationalSocietyId, existingUser.Email);
    }
}

