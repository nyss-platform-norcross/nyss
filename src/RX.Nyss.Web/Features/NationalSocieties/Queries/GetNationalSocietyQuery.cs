using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.NationalSocieties.Queries
{
    public class GetNationalSocietyQuery : IRequest<Result<NationalSocietyResponseDto>>
    {
        public GetNationalSocietyQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public class Handler : IRequestHandler<GetNationalSocietyQuery, Result<NationalSocietyResponseDto>>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            public Handler(INyssContext nyssContext, IAuthorizationService authorizationService)
            {
                _nyssContext = nyssContext;
                _authorizationService = authorizationService;
            }

            public async Task<Result<NationalSocietyResponseDto>> Handle(GetNationalSocietyQuery request, CancellationToken cancellationToken)
            {
                var currentUserName = _authorizationService.GetCurrentUserName();

                var nationalSociety = await _nyssContext.NationalSocieties
                    .Select(n => new NationalSocietyResponseDto
                    {
                        Id = n.Id,
                        ContentLanguageId = n.ContentLanguage.Id,
                        ContentLanguageName = n.ContentLanguage.DisplayName,
                        Name = n.Name,
                        CountryId = n.Country.Id,
                        CountryName = n.Country.Name,
                        IsCurrentUserHeadManager = n.Organizations.Any(o => o.HeadManager.EmailAddress == currentUserName),
                        IsArchived = n.IsArchived,
                        HasCoordinator = n.NationalSocietyUsers.Any(nsu => nsu.User.Role == Role.Coordinator),
                        EpiWeekStartDay = n.EpiWeekStartDay,
                        EnableEidsrIntegration = n.EnableEidsrIntegration,
                    })
                    .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

                return nationalSociety != null
                    ? Result.Success(nationalSociety)
                    : Result.Error<NationalSocietyResponseDto>(ResultKey.NationalSociety.NotFound);
            }
        }
    }
}
