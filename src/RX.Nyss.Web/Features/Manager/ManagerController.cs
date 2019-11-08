using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.Manager.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Manager
{
    [Route("api")]
    public class ManagerController : BaseController
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        /// <summary>
        /// Register a data manager.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the data manager should be registered in</param>
        /// <param name="createManagerRequestDto">The data manager to be created</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/{nationalSocietyId:int}/dataManager/create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateManager(int nationalSocietyId, [FromBody]CreateManagerRequestDto createManagerRequestDto) =>
            await _managerService.CreateManager(nationalSocietyId, createManagerRequestDto);

        /// <summary>
        /// Get a data manager.
        /// </summary>
        /// <param name="dataManagerId">The ID of the requested data manager</param>
        /// <returns></returns>
        [HttpGet("nationalSociety/dataManager/{dataManagerId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ManagerAccess)]
        public async Task<Result> Get(int dataManagerId) =>
            await _managerService.GetManager(dataManagerId);

        /// <summary>
        /// Update a data manager.
        /// </summary>
        /// <param name="dataManagerId">The id of the data manager to be edited</param>
        /// <param name="editManagerRequestDto">The data used to update the specified data manager</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/dataManager/{dataManagerId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ManagerAccess)]
        public async Task<Result> Edit(int dataManagerId, [FromBody]EditManagerRequestDto editManagerRequestDto) =>
            await _managerService.UpdateManager(dataManagerId, editManagerRequestDto);

        /// <summary>
        /// Delete a data manager.
        /// </summary>
        /// <param name="dataManagerId">The ID of the data manager to be deleted</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/dataManager/{dataManagerId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ManagerAccess)]
        public async Task<Result> Delete(int dataManagerId) =>
            await _managerService.DeleteManager(dataManagerId);
    }
}

