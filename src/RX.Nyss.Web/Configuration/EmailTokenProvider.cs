using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RX.Nyss.Web.Configuration
{
    public class EmailTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
        where TUser : class
    {
        public EmailTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<EmailTokenProviderOptions> options,
            ILogger<EmailTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class EmailTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public EmailTokenProviderOptions()
        {
            Name = "Email";
            TokenLifespan = TimeSpan.FromDays(10);
        }
    }
}
