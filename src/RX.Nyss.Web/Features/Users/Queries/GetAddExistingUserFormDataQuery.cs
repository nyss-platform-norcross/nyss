using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Users.Dto;

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

            public Handler(INyssContext nyssContext)
            {
                _nyssContext = nyssContext;
            }

            public async Task<AddExistingUserFormDataDto> Handle(GetAddExistingUserFormDataQuery request, CancellationToken cancellationToken)
            {
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

                return new AddExistingUserFormDataDto
                {
                    Modems = modems,
                    Organizations = nationalSociety.Organizations.Select(o => new OrganizationsDto
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
