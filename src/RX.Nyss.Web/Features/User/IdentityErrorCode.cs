namespace RX.Nyss.Web.Features.User
{
    public enum IdentityErrorCode
    {
        DefaultError,
        ConcurrencyFailure,
        PasswordMismatch,
        InvalidToken,
        LoginAlreadyAssociated,
        InvalidUserName,
        InvalidEmail,
        DuplicateUserName,
        DuplicateEmail,
        InvalidRoleName,
        DuplicateRoleName,
        UserAlreadyHasPassword,
        UserLockoutNotEnabled, 
        UserAlreadyInRole,
        UserNotInRole,
        PasswordTooShort,
        PasswordRequiresNonAlphanumeric,
        PasswordRequiresDigit,
        PasswordRequiresLower,
        PasswordRequiresUpper
    }
}
