using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.DataCollectors.Access
{
    public class MultipleDataCollectorsAccessHandler : ResourceMultipleAccessHandler<MultipleDataCollectorsAccessHandler>
    {
        private readonly IDataCollectorAccessService _dataCollectorAccessService;

        public MultipleDataCollectorsAccessHandler(IHttpContextAccessor httpContextAccessor, IDataCollectorAccessService dataCollectorAccessService)
            : base("dataCollectorIds", httpContextAccessor)
        {
            _dataCollectorAccessService = dataCollectorAccessService;
        }

        protected override Task<bool> HasAccess(IEnumerable<int> dataCollectorIds) =>
            _dataCollectorAccessService.HasCurrentUserAccessToDataCollectors(dataCollectorIds);
    }
}
