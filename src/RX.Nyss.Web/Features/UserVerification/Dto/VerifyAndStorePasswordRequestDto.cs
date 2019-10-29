namespace RX.Nyss.Web.Features.UserVerification.Dto
{
    public class VerifyAndStorePasswordRequestDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
    }
}
