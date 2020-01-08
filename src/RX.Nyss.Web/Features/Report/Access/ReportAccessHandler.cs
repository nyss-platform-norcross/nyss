using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Report.Access
{

    public class ReportAccessRequirement : IAuthorizationRequirement
    {
    }

    public class ReportAccessHandler : AuthorizationHandler<ReportAccessRequirement>
    {
        private const string RouteParameterName = "reportId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReportAccessService _reportAccessService;

        public ReportAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            IReportAccessService reportAccessService)
        {
            _httpContextAccessor = httpContextAccessor;
            _reportAccessService = reportAccessService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ReportAccessRequirement requirement)
        {
            var reportId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (reportId.HasValue && await _reportAccessService.HasCurrentUserAccessToReport(reportId.Value))
            {
                context.Succeed(requirement);
            }
        }
    }
}
