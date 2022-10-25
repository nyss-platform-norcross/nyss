using System;

namespace RX.Nyss.Data.Repositories;

public class EidsrReportData
{
    public string OrgUnit { get; set; }

    public DateTime EventDate { get; set; }

    public string Location	{ get; set; }

    public DateTime DateOfOnset { get; set; }

    public string PhoneNumber { get; set; }

    public string SuspectedDisease	{ get; set; }

    public string EventType { get; set; }

    public string Gender { get; set; }
}