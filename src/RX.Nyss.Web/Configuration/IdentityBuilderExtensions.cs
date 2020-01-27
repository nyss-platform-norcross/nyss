using Microsoft.AspNetCore.Identity;

namespace RX.Nyss.Web.Configuration
{
    public static class IdentityBuilderExtenstions
    {
        public static IdentityBuilder AddEmailLoginProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var provider= typeof(EmailTokenProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider("Email", provider);
        }
    }
}
