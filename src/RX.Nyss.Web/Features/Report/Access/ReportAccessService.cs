using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.Access;
using RX.Nyss.Web.Features.Project.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Report.Access
{
    public interface IReportAccessService
    {
        Task<bool> HasCurrentUserAccessToReport(int reportId);
    }

    public class ReportAccessService: IReportAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IProjectAccessService _projectAccessService;

        public ReportAccessService(IProjectAccessService poProjectAccessService, INyssContext nyssContext)
        {
            _projectAccessService = poProjectAccessService;
            _nyssContext = nyssContext;
        }

        public async Task<bool> HasCurrentUserAccessToReport(int reportId)
        {
            var reportProjectId = await _nyssContext.Reports.Select(r => r.ProjectHealthRisk.Project.Id).FirstOrDefaultAsync();
            return await _projectAccessService.HasCurrentUserAccessToProject(reportProjectId);
        }
    }

}
