namespace RX.Nyss.Web.Features.UserVerification.Dto
{
    public class VerifyAndStorePasswordRequestDto
    {
        public string Email { get; set; }
        public string VerificationToken { get; set; }
        public string NewPassword { get; set; }
    }
}
