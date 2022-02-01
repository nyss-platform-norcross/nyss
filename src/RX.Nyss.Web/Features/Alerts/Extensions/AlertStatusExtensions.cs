using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;

namespace RX.Nyss.Web.Features.Alerts
{
    public static class AlertStatusExtensions
    {
        public static AlertAssessmentStatus GetAssessmentStatus(this AlertStatus alertStatus, int acceptedReports, int pendingReports, int countThreshold) =>
            alertStatus switch
            {
                AlertStatus.Escalated => AlertAssessmentStatus.Escalated,
                AlertStatus.Dismissed => AlertAssessmentStatus.Dismissed,
                AlertStatus.Closed => AlertAssessmentStatus.Closed,
                AlertStatus.Open when acceptedReports >= countThreshold => AlertAssessmentStatus.ToEscalate,
                AlertStatus.Open when acceptedReports + pendingReports >= countThreshold => AlertAssessmentStatus.Open,
                _ => AlertAssessmentStatus.ToDismiss
            };
    }
}
