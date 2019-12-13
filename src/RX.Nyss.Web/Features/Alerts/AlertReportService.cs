using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;
using Message = Microsoft.Azure.ServiceBus.Message;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertReportService
    {
        Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId);
        Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId);
    }

    public class AlertReportService : IAlertReportService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAlertService _alertService;
        private readonly QueueClient _queueClient;

        public AlertReportService(
            IConfig config,
            INyssContext nyssContext,
            IAlertService alertService)
        {
            _nyssContext = nyssContext;
            _alertService = alertService;
            _queueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.ReportDismissalQueue);
        }

        public async Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId)
        {
            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (alertReport.Alert.Status != AlertStatus.Pending)
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReportWrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Pending)
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReportWrongReportStatus);
            }

            alertReport.Report.Status = ReportStatus.Accepted;
            await _nyssContext.SaveChangesAsync();

            var response = new AcceptReportResponseDto
            {
                AssessmentStatus = await _alertService.GetAlertAssessmentStatus(alertId)
            };

            return Success(response);
        }

        public async Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId)
        {
            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (alertReport.Alert.Status != AlertStatus.Pending)
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.AcceptReportWrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Pending)
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.AcceptReportWrongReportStatus);
            }

            alertReport.Report.Status = ReportStatus.Rejected;

            await SendToQueue(new DismissReportMessage { ReportId = reportId });

            await _nyssContext.SaveChangesAsync();

            var response = new DismissReportResponseDto
            {
                AssessmentStatus = await _alertService.GetAlertAssessmentStatus(alertId)
            };

            return Success(response);
        }

        private async Task SendToQueue<T>(T data)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            {
                Label = "RX.Nyss.Web",
            };

            await _queueClient.SendAsync(message);
        }
    }
}
