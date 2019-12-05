using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.Manager.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Manager
{
    [Route("api/manager")]
    public class ManagerController : BaseController
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        /// <summary>
        /// Register a manager.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the manager should be registered in</param>
        /// <param name="createManagerRequestDto">The manager to be created</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateManager(int nationalSocietyId, [FromBody]CreateManagerRequestDto createManagerRequestDto) =>
            await _managerService.CreateManager(nationalSocietyId, createManagerRequestDto);

        /// <summary>
        /// Get a manager.
        /// </summary>
        /// <param name="managerId">The ID of the requested manager</param>
        /// <returns></returns>
        [HttpGet("{managerId:int}/get")]
        //[NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ManagerAccess)]
        public async Task<Result> Get(int managerId) =>
            await _managerService.GetManager(managerId);

        /// <summary>
        /// Update a manager.
        /// </summary>
        /// <param name="managerId">The id of the manager to be edited</param>
        /// <param name="editManagerRequestDto">The data used to update the specified manager</param>
        /// <returns></returns>
        [HttpPost("{managerId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ManagerAccess)]
        public async Task<Result> Edit(int managerId, [FromBody]EditManagerRequestDto editManagerRequestDto) =>
            await _managerService.UpdateManager(managerId, editManagerRequestDto);

        /// <summary>
        /// Delete a manager.
        /// </summary>
        /// <param name="managerId">The ID of the manager to be deleted</param>
        /// <returns></returns>
        [HttpPost("{managerId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ManagerAccess)]
        public async Task<Result> Delete(int managerId) =>
            await _managerService.DeleteManager(managerId);
    }
}

