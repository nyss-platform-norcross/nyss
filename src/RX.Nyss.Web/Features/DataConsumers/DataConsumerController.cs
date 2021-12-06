using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.DataConsumers.Commands;
using RX.Nyss.Web.Features.DataConsumers.Queries;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.DataConsumers
{
    [Route("api/dataConsumer")]
    public class DataConsumerController : BaseController
    {
        /// <summary>
        /// Register a data consumer.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the data consumer should be registered in</param>
        /// <param name="cmd">The data consumer to be created</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Create(int nationalSocietyId, [FromBody] CreateDataConsumerCommand cmd)
        {
            cmd.NationalSocietyId = nationalSocietyId;

            return await Sender.Send(cmd);
        }

        /// <summary>
        /// Get a data consumer
        /// </summary>
        /// <param name="dataConsumerId">The ID of the requested data consumer</param>
        /// <returns></returns>
        [HttpGet("{dataConsumerId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Get(int dataConsumerId) =>
            await Sender.Send(new GetDataConsumerQuery(dataConsumerId));

        /// <summary>
        /// Update a data consumer.
        /// </summary>
        /// <param name="dataConsumerId">The id of the data consumer to be edited</param>
        /// <param name="cmd">The data used to update the specified data consumer</param>
        /// <returns></returns>
        [HttpPost("{dataConsumerId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess)]
        public async Task<Result> Edit(int dataConsumerId, [FromBody] EditDataConsumerCommand cmd)
        {
            cmd.Id = dataConsumerId;

            return await Sender.Send(cmd);
        }

        /// <summary>
        /// Delete a data consumer in a national society.
        /// If the data consumer is also in other national societies, he/she will be be removed from the provided national society,
        /// but the user will not be deleted.
        /// If this is the only national society of the data consumer, the data consumer will be deleted.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the data consumer should be deleted from</param>
        /// <param name="dataConsumerId">The ID of the data consumer to be deleted</param>
        /// <returns></returns>
        [HttpPost("{dataConsumerId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DataConsumerAccess), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Delete(int nationalSocietyId, int dataConsumerId) =>
            await Sender.Send(new DeleteDataConsumerCommand(dataConsumerId, nationalSocietyId));
    }
}
