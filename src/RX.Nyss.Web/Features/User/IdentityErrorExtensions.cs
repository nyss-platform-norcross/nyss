using System;
using Microsoft.AspNetCore.Identity;

namespace RX.Nyss.Web.Features.User
{
    public static class IdentityErrorExtensions
    {
        public static bool IsPasswordTooWeak(this IdentityError identityError)
        {
            switch (Enum.Parse<IdentityErrorCode>(identityError.Code))
            {
                case IdentityErrorCode.PasswordTooShort: 
                case IdentityErrorCode.PasswordRequiresNonAlphanumeric:
                case IdentityErrorCode.PasswordRequiresDigit:
                case IdentityErrorCode.PasswordRequiresLower:
                case IdentityErrorCode.PasswordRequiresUpper:
                    return true;
                default:
                    return false;
            }
        }
    }
}
