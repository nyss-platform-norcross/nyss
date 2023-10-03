using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestData
{
    public string OrgUnit { get; set; }

    public string EventDate { get; set; }

    public string Location { get; set; }

    public string DateOfOnset { get; set; }

    public string PhoneNumber { get; set; }

    public string SuspectedDisease { get; set; }

    public string EventType { get; set; }

    public string Gender { get; set; }
}
