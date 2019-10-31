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
        /// Register a data manager.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the data manager should be registered in</param>
        /// <param name="createDataManagerRequestDto">The data manager to be created</param>
        /// <returns></returns>
        [HttpPost("/api/nationalSociety/{nationalSocietyId:int}/dataManager/create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateDataManager(int nationalSocietyId, [FromBody]CreateDataManagerRequestDto createDataManagerRequestDto) =>
            await _dataManagerService.CreateDataManager(nationalSocietyId, createDataManagerRequestDto);

        /// <summary>
        /// Get a data manager.
        /// </summary>
        /// <param name="dataManagerId">The ID of the requested data manager</param>
        /// <returns></returns>
        [HttpGet("/api/nationalSociety/dataManager/{dataManagerId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataManagerAccess)]
        public async Task<Result> Get(int dataManagerId) =>
            await _dataManagerService.GetDataManager(dataManagerId);

        /// <summary>
        /// Update a data manager.
        /// </summary>
        /// <param name="dataManagerId">The id of the data manager to be edited</param>
        /// <param name="editDataManagerRequestDto">The data used to update the specified data manager</param>
        /// <returns></returns>
        [HttpPost("/api/nationalSociety/dataManager/{dataManagerId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataManagerAccess)]
        public async Task<Result> Edit(int dataManagerId, [FromBody]EditDataManagerRequestDto editDataManagerRequestDto) =>
            await _dataManagerService.UpdateDataManager(dataManagerId, editDataManagerRequestDto);

        /// <summary>
        /// Delete a data manager.
        /// </summary>
        /// <param name="dataManagerId">The ID of the data manager to be deleted</param>
        /// <returns></returns>
        [HttpGet("/api/nationalSociety/dataManager/{dataManagerId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataManagerAccess)]
        public async Task<Result> Delete(int dataManagerId) =>
            await _dataManagerService.DeleteDataManager(dataManagerId);
    }
}

