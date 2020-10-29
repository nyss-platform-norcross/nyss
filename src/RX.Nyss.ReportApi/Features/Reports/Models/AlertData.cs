using System.Collections.Generic;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Features.Reports.Models
{
    public class AlertData
    {
        public Alert Alert { get; set; }

        public List<SupervisorUser> SupervisorsAddedToExistingAlert { get; set; }

        public bool IsExistingAlert { get; set; }
    }
}
