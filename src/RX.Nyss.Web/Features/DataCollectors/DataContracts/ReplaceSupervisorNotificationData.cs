
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts;

public class ReplaceSupervisorNotificationData
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public GatewaySetting GatewaySetting { get; set; }
}
