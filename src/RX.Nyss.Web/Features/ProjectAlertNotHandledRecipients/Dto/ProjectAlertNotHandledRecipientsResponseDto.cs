using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto
{
    public class ProjectAlertNotHandledRecipientsResponseDto
    {
        public List<RecipientDto> Users { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
    }
}
