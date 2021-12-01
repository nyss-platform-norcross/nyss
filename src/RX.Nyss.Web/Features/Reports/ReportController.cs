using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Reports.Commands;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Reports.Queries;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Reports
{
    [Route("api/report")]
    public class ReportController : BaseController
    {
        private readonly IReportService _reportService;

        private readonly IReportSenderService _reportSenderService;

        public ReportController(
            IReportService reportService,
            IReportSenderService reportSenderService)
        {
            _reportService = reportService;
            _reportSenderService = reportSenderService;
        }

        /// <summary>
        /// Gets a report.
        /// </summary>
        /// <param name="reportId">An identifier of a report</param>
        /// <returns>A report</returns>
        [HttpGet("{reportId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.HeadSupervisor, Role.Supervisor), NeedsPolicy(Policy.ReportAccess)]
        public Task<Result<ReportResponseDto>> Get(int reportId) =>
            _reportService.Get(reportId);

        /// <summary>
        /// Gets a list of reports in a project.
        /// </summary>
        [HttpPost("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, [FromBody] ReportListFilterRequestDto filterRequest) =>
            await _reportService.List(projectId, pageNumber, filterRequest);

        /// <summary>
        /// Gets filters data for the project report list.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        [HttpGet("filters")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<ReportListFilterResponseDto>> GetFilters(int projectId) =>
            await _reportService.GetFilters(projectId);

        /// <summary>
        /// Export the list of reports in a project to a csv file.
        /// </summary>
        /// <param name="projectId">The ID of the project to export the reports from</param>
        /// <param name="filterRequest">The filters object</param>
        [HttpPost("exportToCsv")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> ExportToCsv(int projectId, [FromBody] ReportListFilterRequestDto filterRequest) =>
            await Sender.Send(new ExportReportCsvQuery(projectId, filterRequest)).AsFileResult();

        /// <summary>
        /// Export the list of reports in a project to a xlsx file.
        /// </summary>
        /// <param name="projectId">The ID of the project to export the reports from</param>
        /// <param name="filterRequest">The filters object</param>
        [HttpPost("exportToExcel")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> ExportToExcel(int projectId, [FromBody] ReportListFilterRequestDto filterRequest) =>
            await Sender.Send(new ExportReportExcelQuery(projectId, filterRequest)).AsFileResult();

        /// <summary>
        /// Mark the selected report as error.
        /// </summary>
        /// <param name="reportId">The ID of the report to be marked as error</param>
        [HttpPost("{reportId:int}/markAsError")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ReportAccess)]
        public async Task<Result> MarkAsError(int reportId) =>
            await _reportService.MarkAsError(reportId);

        /// <summary>
        /// Gets human health risks for the project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        [HttpGet("humanHealthRisksForProject/{projectId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.HeadSupervisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<HumanHealthRiskResponseDto>> GetHumanHealthRisksForProject(int projectId) =>
            await _reportService.GetHumanHealthRisksForProject(projectId);

        /// <summary>
        /// Edits a report.
        /// </summary>
        /// <param name="reportId">An identifier of a report</param>
        /// <param name="reportRequestDto">A report</param>
        [HttpPost("{reportId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.HeadSupervisor, Role.Supervisor), NeedsPolicy(Policy.ReportAccess)]
        public async Task<Result> Edit(int reportId, [FromBody] ReportRequestDto reportRequestDto) =>
            await _reportService.Edit(reportId, reportRequestDto);

        /// <summary>
        /// Sends a report for testing purposes.
        /// </summary>
        /// <param name="report">The report to send</param>
        [HttpPost("sendReport")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor)]
        public async Task<Result> SendReport([FromBody]SendReportRequestDto report) =>
            await _reportSenderService.SendReport(report);

        /// <summary>
        /// Gets send report form data
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society</param>
        /// <returns>Form data for sending report</returns>
        [HttpGet("formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor, Role.HeadSupervisor)]
        public async Task<Result<SendReportFormDataDto>> GetFormData(int nationalSocietyId) =>
            await _reportSenderService.GetFormData(nationalSocietyId);

        /// <summary>
        /// Keeps the selected report.
        /// </summary>
        /// <param name="reportId">The ID of the report to be kept</param>
        [HttpPost("{reportId:int}/accept")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ReportAccess)]
        public async Task<Result> AcceptReport(int reportId) =>
            await _reportService.AcceptReport(reportId);

        /// <summary>
        /// Dismisses the selected report.
        /// </summary>
        /// <param name="reportId">The ID of the report to be dismissed</param>
        [HttpPost("{reportId:int}/dismiss")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor), NeedsPolicy(Policy.ReportAccess)]
        public async Task<Result> DismissReport(int reportId) =>
            await _reportService.DismissReport(reportId);

        /// <summary>
        /// Marks the report as corrected
        /// </summary>
        /// <param name="reportId">The ID of the report to be corrected</param>
        /// <returns></returns>
        [HttpPost("{reportId:int}/markAsCorrected")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor)]
        public async Task<Result> Correct(int reportId) =>
            await Sender.Send(new CorrectReportCommand(reportId));

        /// <summary>
        /// Marks the report as not corrected
        /// </summary>
        /// <param name="reportId">The ID of the report to be not corrected</param>
        /// <returns></returns>
        [HttpDelete("{reportId:int}/markAsCorrected")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor, Role.HeadSupervisor)]
        public async Task<Result> UndoCorrect(int reportId) =>
            await Sender.Send(new UndoCorrectReportCommand(reportId));
    }
}
