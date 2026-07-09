using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Users.Commands
{
    public class AddExistingUserCommand : IRequest<Result>
    {
        public int NationalSocietyId { get; set; }

        public string Email { get; set; }

        public int? ModemId { get; set; }

        public int OrganizationId { get; set; }

        public class Handler : IRequestHandler<AddExistingUserCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            public Handler(INyssContext nyssContext, IAuthorizationService authorizationService)
            {
                _nyssContext = nyssContext;
                _authorizationService = authorizationService;
            }

            public async Task<Result> Handle(AddExistingUserCommand request, CancellationToken cancellationToken)
            {
                var nationalSocietyId = request.NationalSocietyId;

                var currentUser = await _authorizationService.GetCurrentUser();
                var user = await _nyssContext.Users.FilterAvailable()
                    .Where(u => u.EmailAddress == request.Email)
                    .SingleOrDefaultAsync(cancellationToken);
                var organization = await _nyssContext.Organizations.SingleOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

                if (user == null)
                {
                    return Error(ResultKey.User.Registration.UserNotFound);
                }

                if (organization == null)
                {
                    return Error(ResultKey.User.Registration.OrganizationDoesNotExists);
                }

                if (user.Role != Role.TechnicalAdvisor && user.Role != Role.DataConsumer)
                {
                    return Error(ResultKey.User.Registration.NoAssignableUserWithThisEmailFound);
                }

                if (user.Role == Role.TechnicalAdvisor && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor))
                {
                    return Error(ResultKey.User.Registration.TechnicalAdvisorsCanBeAttachedOnlyByManagers);
                }

                var userAlreadyIsInThisNationalSociety = await _nyssContext.UserNationalSocieties
                    .FilterAvailableUsers()
                    .AnyAsync(uns => uns.NationalSocietyId == nationalSocietyId && uns.UserId == user.Id, cancellationToken);

                if (userAlreadyIsInThisNationalSociety)
                {
                    return Error(ResultKey.User.Registration.UserIsAlreadyInThisNationalSociety);
                }

                var nationalSocietyIsArchived = await _nyssContext.NationalSocieties
                    .AnyAsync(ns => ns.Id == nationalSocietyId && ns.IsArchived, cancellationToken);

                if (nationalSocietyIsArchived)
                {
                    return Error(ResultKey.User.Registration.CannotAddExistingUsersToArchivedNationalSociety);
                }

                if (_authorizationService.IsCurrentUserInRole(Role.Manager)
                    && !await IsInOrganization(currentUser.Id, request.NationalSocietyId, organization.Id))
                {
                    return Error(ResultKey.User.Registration.InvalidUserOrganization);
                }

                var userNationalSociety = new UserNationalSociety
                {
                    NationalSocietyId = nationalSocietyId,
                    UserId = user.Id,
                    Organization = organization,
                };

                if (request.ModemId.HasValue && user.Role == Role.TechnicalAdvisor)
                {
                    await AddModemToExistingTechnicalAdvisor((TechnicalAdvisorUser)user, request.ModemId.Value, nationalSocietyId);
                }

                _nyssContext.UserNationalSocieties.Add(userNationalSociety);

                await _nyssContext.SaveChangesAsync(cancellationToken);

                return Success();
            }

            private async Task<bool> IsInOrganization(int userId, int nationalSocietyId, int organizationId) =>
                await _nyssContext.UserNationalSocieties.AnyAsync(
                    uns => uns.UserId == userId && uns.NationalSocietyId == nationalSocietyId && uns.OrganizationId == organizationId);

            private async Task AddModemToExistingTechnicalAdvisor(TechnicalAdvisorUser user, int modemId, int nationalSocietyId)
            {
                var technicalAdvisorModem = new TechnicalAdvisorUserGatewayModem
                {
                    GatewayModem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId),
                    TechnicalAdvisorUser = user
                };

                if (technicalAdvisorModem.GatewayModem == null)
                {
                    throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
                }

                if (user.TechnicalAdvisorUserGatewayModems == null)
                {
                    user.TechnicalAdvisorUserGatewayModems = new List<TechnicalAdvisorUserGatewayModem> { technicalAdvisorModem };
                }
                else
                {
                    user.TechnicalAdvisorUserGatewayModems.Add(technicalAdvisorModem);
                }
            }
        }

        public class Validator : AbstractValidator<AddExistingUserCommand>
        {
            public Validator()
            {
                RuleFor(m => m.NationalSocietyId).GreaterThan(0);
                RuleFor(m => m.OrganizationId).GreaterThan(0);
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(m => m.ModemId).GreaterThan(0).When(m => m.ModemId.HasValue);
            }
        }
    }
}
