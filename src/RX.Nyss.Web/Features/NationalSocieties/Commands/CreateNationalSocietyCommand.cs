using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSocieties.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.NationalSocieties.Commands
{
    public class CreateNationalSocietyCommand : IRequest<Result>
    {
        public string Name { get; set; }

        public int CountryId { get; set; }

        public int ContentLanguageId { get; set; }

        public string InitialOrganizationName { get; set; }

        public DayOfWeek EpiWeekStartDay { get; set; }

        public class Handler : IRequestHandler<CreateNationalSocietyCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly ILoggerAdapter _loggerAdapter;

            public Handler(INyssContext nyssContext, ILoggerAdapter loggerAdapter)
            {
                _nyssContext = nyssContext;
                _loggerAdapter = loggerAdapter;
            }

            public async Task<Result> Handle(CreateNationalSocietyCommand request, CancellationToken cancellationToken)
            {
                var contentLanguage = await _nyssContext.ContentLanguages.FindAsync(request.ContentLanguageId);
                var country = await _nyssContext.Countries.FindAsync(request.CountryId);

                var nationalSociety = new NationalSociety
                {
                    Name = request.Name,
                    ContentLanguage = contentLanguage,
                    Country = country,
                    IsArchived = false,
                    StartDate = DateTime.UtcNow,
                    EpiWeekStartDay = request.EpiWeekStartDay,
                };

                await _nyssContext.AddAsync(nationalSociety, cancellationToken);
                await _nyssContext.SaveChangesAsync(cancellationToken);

                nationalSociety.DefaultOrganization = new Organization
                {
                    Name = request.InitialOrganizationName,
                    NationalSociety = nationalSociety
                };

                await _nyssContext.SaveChangesAsync(cancellationToken);

                _loggerAdapter.Info($"A national society {nationalSociety} was created");

                return Result.Success(nationalSociety.Id);
            }
        }

        public class Validator : AbstractValidator<CreateNationalSocietyCommand>
        {
            public Validator(INationalSocietyValidationService nationalSocietyValidationService)
            {
                RuleFor(r => r.Name).NotEmpty().MinimumLength(3);
                RuleFor(r => r.Name)
                    .MustAsync(async (_, name, _) => !await nationalSocietyValidationService.NameExists(name))
                    .WithMessageKey(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                RuleFor(r => r.ContentLanguageId).GreaterThan(0);
                RuleFor(r => r.ContentLanguageId)
                    .MustAsync(async (_, languageId, _) => await nationalSocietyValidationService.LanguageExists(languageId))
                    .WithMessageKey(ResultKey.NationalSociety.Creation.LanguageNotFound);
                RuleFor(r => r.CountryId).GreaterThan(0);
                RuleFor(r => r.CountryId)
                    .MustAsync(async (_, countryId, _) => await nationalSocietyValidationService.CountryExists(countryId))
                    .WithMessageKey(ResultKey.NationalSociety.Creation.CountryNotFound);
                RuleFor(r => r.InitialOrganizationName).NotEmpty().MaximumLength(100);
            }
        }
    }
}
