using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.NationalSociety.User.DataManager.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager
{
    [Route("api/nationalSociety/dataManager")]
    public class DataManagerController
    {
        private readonly IDataManagerService _dataManagerService;

        public DataManagerController(IDataManagerService dataManagerService)
        {
            _dataManagerService = dataManagerService;
        }

        /// <summary>
        /// Register a data consumer.
        /// </summary>
        /// <param name="createDataManagerRequestDto">The data consumer to be created</param>
        /// <param name="nationalSocietyId">The ID of the national society the data consumer should be registered in</param>
        /// <returns></returns>
        [HttpPost("create"), NeedsRole(Role.GlobalCoordinator, Role.DataManager, Role.DataManager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateDataManager([FromQuery]int nationalSocietyId, [FromBody]CreateDataManagerRequestDto createDataManagerRequestDto) =>
            await _dataManagerService.CreateDataManager(nationalSocietyId, createDataManagerRequestDto);

        /// <summary>
        /// Get a data consumer
        /// </summary>
        /// <param name="id">The ID of the requested data consumer</param>
        /// <returns></returns>
        [HttpGet("get"), NeedsRole(Role.GlobalCoordinator, Role.DataManager, Role.DataManager), NeedsPolicy(Policy.DataManagerAccess)]
        public async Task<Result> Get(int id) =>
            await _dataManagerService.GetDataManager(id);

        /// <summary>
        /// Update a data consumer.
        /// </summary>
        /// <param name="editDataManagerRequestDto">The data consumer to be updated</param>
        /// <returns></returns>
        [HttpPost("edit"), NeedsRole(Role.GlobalCoordinator, Role.DataManager, Role.DataManager), NeedsPolicy(Policy.DataManagerAccess)]
        public async Task<Result> Edit([FromBody]EditDataManagerRequestDto editDataManagerRequestDto) =>
            await _dataManagerService.UpdateDataManager(editDataManagerRequestDto);

        /// <summary>
        /// Delete a technical advisor.
        /// </summary>
        /// <param name="id">The ID of the technical advisor to be deleted</param>
        /// <returns></returns>
        [HttpGet("delete"), NeedsRole(Role.GlobalCoordinator, Role.DataManager, Role.DataManager), NeedsPolicy(Policy.DataManagerAccess)]
        public async Task<Result> Delete(int id) =>
            await _dataManagerService.DeleteDataManager(id);
    }
}

