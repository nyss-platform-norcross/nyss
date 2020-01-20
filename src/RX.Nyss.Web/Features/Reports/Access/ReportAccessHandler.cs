using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Reports.Access
{
    public class ReportAccessHandler : ResourceAccessHandler<ProjectAccessHandler>
    {
        private readonly IReportAccessService _reportAccessService;

        public ReportAccessHandler(IHttpContextAccessor httpContextAccessor, IReportAccessService reportAccessService)
            : base("reportId", httpContextAccessor)
        {
            _reportAccessService = reportAccessService;
        }

        protected override Task<bool> HasAccess(int reportId) =>
            _reportAccessService.HasCurrentUserAccessToReport(reportId);
    }
}
