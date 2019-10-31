namespace RX.Nyss.Web.Features.UserVerification.Dto
{
    public class ResetPasswordCallbackRequestDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
    }
}
