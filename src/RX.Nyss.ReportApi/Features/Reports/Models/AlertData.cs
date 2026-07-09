using System.Collections.Generic;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Common.Contracts;

namespace RX.Nyss.ReportApi.Features.Reports.Models
{
    public class AlertData
    {
        public Alert Alert { get; set; }

        public List<SupervisorSmsRecipient> SupervisorsAddedToExistingAlert { get; set; }

        public bool IsExistingAlert { get; set; }
    }
}
