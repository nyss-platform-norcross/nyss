using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.DataConsumer.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataConsumer
{
    [Route("api")]
    public class DataConsumerController : BaseController
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
        [HttpPost("nationalSociety/{nationalSocietyId:int}/dataConsumer/create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateDataConsumer(int nationalSocietyId, [FromBody]CreateDataConsumerRequestDto createDataConsumerRequestDto) =>
            await _dataConsumerService.CreateDataConsumer(nationalSocietyId, createDataConsumerRequestDto);

        /// <summary>
        /// Get a data consumer
        /// </summary>
        /// <param name="id">The ID of the requested data consumer</param>
        /// <returns></returns>
        [HttpGet("nationalSociety/dataConsumer/{dataConsumerId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Get(int dataConsumerId) =>
            await _dataConsumerService.GetDataConsumer(dataConsumerId);

        /// <summary>
        /// Update a data consumer.
        /// </summary>
        /// <param name="dataConsumerId">The id of the data consumer to be edited</param>
        /// <param name="editDataConsumerRequestDto">The data used to update the specified data consumer</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/dataConsumer/{dataConsumerId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Edit(int dataConsumerId, [FromBody]EditDataConsumerRequestDto editDataConsumerRequestDto) =>
            await _dataConsumerService.UpdateDataConsumer(dataConsumerId, editDataConsumerRequestDto);

        /// <summary>
        /// Delete a data consumer.
        /// </summary>
        /// <param name="id">The ID of the data consumer to be deleted</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/dataConsumer/{dataConsumerId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Delete(int dataConsumerId) =>
            await _dataConsumerService.DeleteDataConsumer(dataConsumerId);
    }
}

