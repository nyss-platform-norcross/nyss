using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.NationalSociety.User.DataConsumer.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataConsumer
{
    [Route("api/nationalSociety/dataConsumer")]
    public class DataConsumerController
    {
        private readonly IDataConsumerService _dataConsumerService;

        public DataConsumerController(IDataConsumerService dataConsumerService)
        {
            _dataConsumerService = dataConsumerService;
        }

        /// <summary>
        /// Register a data consumer.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the data consumer should be registered in</param>
        /// <param name="createDataConsumerRequestDto">The data consumer to be created</param>
        /// <returns></returns>
        [HttpPost("create"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateDataConsumer([FromQuery]int nationalSocietyId, [FromBody]CreateDataConsumerRequestDto createDataConsumerRequestDto) =>
            await _dataConsumerService.CreateDataConsumer(nationalSocietyId, createDataConsumerRequestDto);

        /// <summary>
        /// Get a data consumer
        /// </summary>
        /// <param name="id">The ID of the requested data consumer</param>
        /// <returns></returns>
        [HttpGet("get"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Get(int id) =>
            await _dataConsumerService.GetDataConsumer(id);

        /// <summary>
        /// Update a data consumer.
        /// </summary>
        /// <param name="editDataConsumerRequestDto">The data consumer to be updated</param>
        /// <returns></returns>
        [HttpPost("edit"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Edit([FromBody]EditDataConsumerRequestDto editDataConsumerRequestDto) =>
            await _dataConsumerService.UpdateDataConsumer(editDataConsumerRequestDto);

        /// <summary>
        /// Delete a data consumer.
        /// </summary>
        /// <param name="id">The ID of the data consumer to be deleted</param>
        /// <returns></returns>
        [HttpGet("delete"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Delete(int id) =>
            await _dataConsumerService.DeleteDataConsumer(id);
    }
}

