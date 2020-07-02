using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.NationalSocieties.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.NationalSocieties.Dto
{
    public class EditNationalSocietyRequestDto
    {
        public string Name { get; set; }
        public int CountryId { get; set; }
        public int ContentLanguageId { get; set; }

        public class Validator : AbstractValidator<EditNationalSocietyRequestDto>
        {
            public Validator(INationalSocietyValidationService nationalSocietyValidationService)
            {
                RuleFor(r => r.Name).NotEmpty().MinimumLength(3);
                RuleFor(r => r.Name)
                    .MustAsync(async (model, name, t) => !await nationalSocietyValidationService.NameExists(name))
                    .WithMessageKey(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                RuleFor(r => r.ContentLanguageId).GreaterThan(0);
                RuleFor(r => r.ContentLanguageId)
                    .MustAsync(async (model, languageId, t) => await nationalSocietyValidationService.LanguageExists(languageId))
                    .WithMessageKey(ResultKey.NationalSociety.Creation.LanguageNotFound);
                RuleFor(r => r.CountryId).GreaterThan(0);
                RuleFor(r => r.CountryId)
                    .MustAsync(async (model, countryId, t) => await nationalSocietyValidationService.CountryExists(countryId))
                    .WithMessageKey(ResultKey.NationalSociety.Creation.CountryNotFound);
            }
        }
    }
}
