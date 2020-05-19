namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Dto
{
    public class ProjectAlertRecipientListResponseDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Organization { get; set; }
    }
}
