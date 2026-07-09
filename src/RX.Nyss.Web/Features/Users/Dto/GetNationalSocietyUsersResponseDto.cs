namespace RX.Nyss.Web.Features.Users.Dto
{
    public class GetNationalSocietyUsersResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public bool IsHeadManager { get; set; }
        public bool IsPendingHeadManager { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public bool IsVerified { get; set; }
    }
}
