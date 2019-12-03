using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.DataConsumer.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.DataConsumer
{
    [Route("api/dataConsumer")]
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
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateDataConsumer(int nationalSocietyId, [FromBody]CreateDataConsumerRequestDto createDataConsumerRequestDto) =>
            await _dataConsumerService.CreateDataConsumer(nationalSocietyId, createDataConsumerRequestDto);

        /// <summary>
        /// Get a data consumer
        /// </summary>
        /// <param name="dataConsumerId">The ID of the requested data consumer</param>
        /// <returns></returns>
        [HttpGet("{dataConsumerId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Get(int dataConsumerId) =>
            await _dataConsumerService.GetDataConsumer(dataConsumerId);

        /// <summary>
        /// Update a data consumer.
        /// </summary>
        /// <param name="dataConsumerId">The id of the data consumer to be edited</param>
        /// <param name="editDataConsumerRequestDto">The data used to update the specified data consumer</param>
        /// <returns></returns>
        [HttpPost("{dataConsumerId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Edit(int dataConsumerId, [FromBody]EditDataConsumerRequestDto editDataConsumerRequestDto) =>
            await _dataConsumerService.UpdateDataConsumer(dataConsumerId, editDataConsumerRequestDto);

        /// <summary>
        /// Remove a data consumer from a national society.
        /// If the data consumer is also in other national societies, he/she will be removed from the provided national society, but the user will not be deleted.
        /// If this is the only national society of the data consumer, the data consumer will be deleted.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the data consumer should be removed from</param>
        /// <param name="dataConsumerId">The ID of the data consumer to be removed</param>
        /// <returns></returns>
        [HttpPost("{dataConsumerId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Delete(int nationalSocietyId, int dataConsumerId) =>
            await _dataConsumerService.DeleteDataConsumer(nationalSocietyId, dataConsumerId, User.GetRoles());
    }
}

