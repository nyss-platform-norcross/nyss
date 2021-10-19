using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Alerts
{
    public static class ReportCaseExtensions
    {
        private const string SexFemale = "Female";

        private const string SexMale = "Male";

        private const string AgeAtLeastFive = "AtLeastFive";

        private const string AgeBelowFive = "BelowFive";

        private const string Unspecified = "Unspecified";

        public static string GetSex(this ReportCase reportedCase)
        {
            if (reportedCase == null)
            {
                return null;
            }

            if (reportedCase.CountFemalesAtLeastFive > 0 || reportedCase.CountFemalesBelowFive > 0)
            {
                return SexFemale;
            }

            if (reportedCase.CountMalesBelowFive > 0 || reportedCase.CountMalesAtLeastFive > 0)
            {
                return SexMale;
            }

            if (reportedCase.CountUnspecifiedSexAndAge > 0)
            {
                return Unspecified;
            }

            throw new ResultException(ResultKey.Alert.InconsistentReportData);
        }

        public static string GetAge(this ReportCase reportedCase)
        {
            if (reportedCase == null)
            {
                return null;
            }

            if (reportedCase.CountFemalesAtLeastFive > 0 || reportedCase.CountMalesAtLeastFive > 0)
            {
                return AgeAtLeastFive;
            }

            if (reportedCase.CountFemalesBelowFive > 0 || reportedCase.CountMalesBelowFive > 0)
            {
                return AgeBelowFive;
            }

            if (reportedCase.CountUnspecifiedSexAndAge > 0)
            {
                return Unspecified;
            }

            throw new ResultException(ResultKey.Alert.InconsistentReportData);
        }
    }
}
