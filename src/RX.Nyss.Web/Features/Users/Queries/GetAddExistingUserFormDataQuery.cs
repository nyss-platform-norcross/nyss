using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Users.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Users.Queries
{
    public class GetAddExistingUserFormDataQuery : IRequest<AddExistingUserFormDataDto>
    {
        public GetAddExistingUserFormDataQuery(int nationalSocietyId)
        {
            NationalSocietyId = nationalSocietyId;
        }

        public int NationalSocietyId { get; }

        public class Handler : IRequestHandler<GetAddExistingUserFormDataQuery, AddExistingUserFormDataDto>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            public Handler(INyssContext nyssContext, IAuthorizationService authorizationService)
            {
                _nyssContext = nyssContext;
                _authorizationService = authorizationService;
            }

            public async Task<AddExistingUserFormDataDto> Handle(GetAddExistingUserFormDataQuery request, CancellationToken cancellationToken)
            {
                var currentUser = await _authorizationService.GetCurrentUser();
                var nationalSociety = await _nyssContext.NationalSocieties
                    .AsNoTracking()
                    .Include(ns => ns.Organizations)
                    .SingleOrDefaultAsync(ns => ns.Id == request.NationalSocietyId, cancellationToken);

                var modems = await _nyssContext.GatewayModems
                    .Where(gm => gm.GatewaySetting.NationalSocietyId == request.NationalSocietyId)
                    .Select(gm => new GatewayModemResponseDto
                    {
                        Id = gm.Id,
                        Name = gm.Name
                    })
                    .ToListAsync(cancellationToken);

                var currentUserOrganizationId = _authorizationService.IsCurrentUserInRole(Role.Manager)
                    ? (await _nyssContext.UserNationalSocieties.SingleOrDefaultAsync(
                        uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == request.NationalSocietyId, cancellationToken))?.OrganizationId
                    : null;

                return new AddExistingUserFormDataDto
                {
                    Modems = modems,
                    Organizations = nationalSociety.Organizations
                        .Where(o => currentUserOrganizationId == null || o.Id == currentUserOrganizationId)
                        .Select(o => new OrganizationsDto
                        {
                            Id = o.Id,
                            Name = o.Name,
                            IsDefaultOrganization = nationalSociety.DefaultOrganizationId == o.Id,
                        }).ToList(),
                };
            }
        }
    }
}
