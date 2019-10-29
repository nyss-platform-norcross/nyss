using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Create;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Edit;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety.User
{
    [Route("api/nationalSociety/User")]
    public class UserController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Register a data manager.
        /// </summary>
        /// <param name="createDataManagerRequestDto">The data manger to be created</param>
        /// <returns></returns>
        [HttpPost("createDataManager"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> CreateDataManager([FromBody]CreateDataManagerRequestDto createDataManagerRequestDto) =>
            await _userService.CreateDataManager(createDataManagerRequestDto);

        /// <summary>
        /// Register a technical advisor.
        /// </summary>
        /// <param name="createNationalSocietyUserRequestDto">The technical advisor to be created</param>
        /// <returns></returns>
        [HttpPost("createTechnicalAdvisor"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> CreateTechnicalAdvisor([FromBody]CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto) =>
            await _userService.CreateTechnicalAdvisor(createTechnicalAdvisorRequestDto);

        /// <summary>
        /// Register a data consumer.
        /// </summary>
        /// <param name="createNationalSocietyUserRequestDto">The data consumer to be created</param>
        /// <returns></returns>
        [HttpPost("createDataConsumer"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> CreateDataConsumer([FromBody]CreateDataConsumerRequestDto createDataConsumerRequestDto) =>
            await _userService.CreateDataConsumer(createDataConsumerRequestDto);

        /// <summary>
        /// Get the data of a user on the national society level.
        /// </summary>
        /// <param name="id">The ID of the requested user</param>
        /// <returns></returns>
        [HttpGet("get"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> Get(int id) =>
            await _userService.GetNationalSocietyUser(id);

        /// <summary>
        /// Update a data manager
        /// </summary>
        /// <param name="editDataManagerRequestDto">The data manager to be updated</param>
        /// <returns></returns>
        [HttpPost("editDataManager"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> Edit([FromBody]EditDataManagerRequestDto editDataManagerRequestDto) =>
            await _userService.UpdateNationalSocietyUser<DataManagerUser>(editDataManagerRequestDto);

        /// <summary>
        /// Update a data consumer.
        /// </summary>
        /// <param name="editDataConsumerRequestDto">The data consumer to be updated</param>
        /// <returns></returns>
        [HttpPost("editDataConsumer"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> Edit([FromBody]EditDataConsumerRequestDto editDataConsumerRequestDto) =>
            await _userService.UpdateNationalSocietyUser<DataConsumerUser>(editDataConsumerRequestDto);

        /// <summary>
        /// Update a technical advisor.
        /// </summary>
        /// <param name="editTechnicalAdvisorRequestDto">The technical advisor to be updated</param>
        /// <returns></returns>
        [HttpPost("editTechnicalAdvisor"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> Edit([FromBody]EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto) =>
            await _userService.UpdateNationalSocietyUser<TechnicalAdvisorUser>(editTechnicalAdvisorRequestDto);

        /// <summary>
        /// Lists all users on the national society level.
        /// </summary>
        /// <returns></returns>
        [HttpGet("list"), NeedsRole(Role.GlobalCoordinator)]
        public async Task<Result> List(int nationalSocietyId) =>
            await _userService.GetUsersInNationalSociety(nationalSocietyId);
    }
}

