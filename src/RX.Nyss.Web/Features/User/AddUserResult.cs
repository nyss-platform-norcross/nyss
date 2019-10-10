namespace RX.Nyss.Web.Features.User
{
    public enum AddUserResult
    {
        Success,
        UserAlreadyExists,
        PasswordTooWeak,
        UnknownError
    }
}
