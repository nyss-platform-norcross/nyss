﻿using System;

namespace RX.Nyss.Common.Services.EidsrClient.Dto;

public class EidsrRegisterEventRequestData
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