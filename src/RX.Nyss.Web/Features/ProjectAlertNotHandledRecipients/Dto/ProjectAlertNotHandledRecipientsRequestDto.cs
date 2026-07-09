using System.Collections.Generic;
namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto;

public class ProjectAlertNotHandledRecipientsRequestDto
{
    public List<ProjectAlertNotHandledRecipientRequestDto> Recipients { get; set; }
}
