using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Services
{
    public static class EmailTextGenerator
    {
        public static (string subject, string body) GenerateResetPasswordEmail(string resetUrl, string name)
        {
            var subject = "Reset you password";
            var body = @"
                <h1>Dear {{name}}</h1>
                <p>To reset your password, click the link below:</p>
                <a href=""{{resetUrl}}"">Reset password</a>
                <p>Kind regards from to the NYSS platform</p>";

            body = body
                .Replace("{{name}}", name)
                .Replace("{{resetUrl}}", resetUrl);

            return (subject, body);
        }

        public static (string subject, string body) GenerateEmailVerificationEmail(Role role, string callbackUrl, string name)
        {
            var roleName = GetRoleName(role);

            // ToDo: Add support for multiple languages
            var subject = "Please verify your email address";
            var body = @"
                <h1>Dear {{username}},</h1>
                <p>
                  You have now been registered in NYSS. Your role is: {{roleName}}.
                  Please follow this link to set your password for login to the NYSS platform.
                </p>

                <a href=""{{link}}"">Verify you email</a>

                <p>
                  Your username and password is private and should not be shared with anyone else.
                </p>
                <p>
                  If you did not expect to be registered, or are not connected to the NYSS platform in any other way, please ignore this
                  E-Mail.
                </p>

                <p>
                  Kind regards and welcome to the NYSS platform!
                </p>
                ";
            body = body
                .Replace("{{username}}", name)
                .Replace("{{roleName}}", roleName)
                .Replace("{{link}}", callbackUrl);

            return (subject, body);
        }

        private static string GetRoleName(Role userRole) =>
            userRole switch
            {
                Role.DataConsumer => "Data consumer",
                Role.GlobalCoordinator => "Global coordinator",
                Role.TechnicalAdvisor => "Technical advisor",
                _ => userRole.ToString()
            };
    }
}
