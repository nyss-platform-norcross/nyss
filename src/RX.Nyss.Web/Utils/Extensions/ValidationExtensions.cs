using FluentValidation;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithMessageKey<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string errorKey) =>
            rule.WithMessage($"string:{errorKey}");
    }
}
