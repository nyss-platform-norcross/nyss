using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.DataCollectors.Access
{
    public class DataCollectorAccessHandler : ResourceAccessHandler<DataCollectorAccessHandler>
    {
        private readonly IDataCollectorAccessService _dataCollectorAccessService;

        public DataCollectorAccessHandler(IHttpContextAccessor httpContextAccessor, IDataCollectorAccessService dataCollectorAccessService)
            : base("dataCollectorId", httpContextAccessor)
        {
            _dataCollectorAccessService = dataCollectorAccessService;
        }

        protected override Task<bool> HasAccess(int dataCollectorId) =>
            _dataCollectorAccessService.HasCurrentUserAccessToDataCollector(dataCollectorId);
    }
}
