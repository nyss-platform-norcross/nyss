using System.Text.RegularExpressions;
using FluentValidation;

namespace RX.Nyss.Web.Services
{
    public static class Validators
    {
        private static readonly Regex PhoneNumberValidatorRegex = new Regex(@"^\+[0-9]{6}[0-9]*$");

        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder) =>
            ruleBuilder.Matches(PhoneNumberValidatorRegex);
    }
}
