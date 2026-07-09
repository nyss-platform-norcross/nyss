namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto
{
    public class ProjectAlertNotHandledRecipientResponseDto
    {
        public int? UserId { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
    }
}
