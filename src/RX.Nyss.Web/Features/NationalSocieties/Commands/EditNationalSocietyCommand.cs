using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.NationalSocieties.Validation;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.NationalSocieties.Commands
{
    public class EditNationalSocietyCommand : IRequest<Result>
    {
        public EditNationalSocietyCommand(int id, RequestBody body)
        {
            Id = id;
            Body = body;
        }

        public int Id { get; }

        public RequestBody Body { get; }

        public class Handler : IRequestHandler<EditNationalSocietyCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            public Handler(IAuthorizationService authorizationService, INyssContext nyssContext)
            {
                _authorizationService = authorizationService;
                _nyssContext = nyssContext;
            }

            public async Task<Result> Handle(EditNationalSocietyCommand request, CancellationToken cancellationToken)
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                var nationalSocietyData = await _nyssContext.NationalSocieties
                    .Where(n => n.Id == request.Id)
                    .Select(ns => new
                    {
                        NationalSociety = ns,
                        CurrentUserOrganizationId = ns.NationalSocietyUsers
                            .Where(uns => uns.User == currentUser)
                            .Select(uns => uns.OrganizationId)
                            .SingleOrDefault(),
                        HasCoordinator = ns.NationalSocietyUsers
                            .Any(uns => uns.User.Role == Role.Coordinator)
                    })
                    .SingleAsync(cancellationToken);

                var nationalSociety = nationalSocietyData.NationalSociety;

                if (nationalSociety.IsArchived)
                {
                    return Result.Error(ResultKey.NationalSociety.Edit.CannotEditArchivedNationalSociety);
                }

                if (nationalSocietyData.HasCoordinator && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
                {
                    return Result.Error(ResultKey.UnexpectedError);
                }

                var contentLanguage = await _nyssContext.ContentLanguages.FindAsync(request.Body.ContentLanguageId);
                var country = await _nyssContext.Countries.FindAsync(request.Body.CountryId);

                nationalSociety.Name = request.Body.Name;
                nationalSociety.ContentLanguage = contentLanguage;
                nationalSociety.Country = country;
                nationalSociety.EpiWeekStartDay = request.Body.EpiWeekStartDay;
                nationalSociety.EnableEidsrIntegration = request.Body.EnableEidsrIntegration;

                await _nyssContext.SaveChangesAsync(cancellationToken);

                return Result.SuccessMessage(ResultKey.NationalSociety.Edit.Success);
            }
        }

        public class RequestBody
        {
            public string Name { get; set; }

            public int CountryId { get; set; }

            public int ContentLanguageId { get; set; }

            public DayOfWeek EpiWeekStartDay { get; set; }

            public bool EnableEidsrIntegration { get; set; }

            public int Id { get; set; }
        }

        public class Validator : AbstractValidator<RequestBody>
        {
            public Validator(INationalSocietyValidationService nationalSocietyValidationService)
            {
                RuleFor(r => r.Name).NotEmpty().MinimumLength(3);
                RuleFor(r => r.Name)
                    .MustAsync(async (model, name, t) => !await nationalSocietyValidationService.NameExistsToOther(name, model.Id))
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
