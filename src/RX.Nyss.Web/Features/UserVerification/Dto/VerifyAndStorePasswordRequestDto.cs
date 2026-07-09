namespace RX.Nyss.Web.Features.UserVerification.Dto
{
    public class VerifyAndStorePasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
