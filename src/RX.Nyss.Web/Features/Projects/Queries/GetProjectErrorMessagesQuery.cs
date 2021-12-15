using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Projects.Dto;

namespace RX.Nyss.Web.Features.Projects.Queries
{
    public class GetProjectErrorMessagesQuery : IRequest<IReadOnlyList<ProjectErrorMessageDto>>
    {
        public GetProjectErrorMessagesQuery(int projectId)
        {
            ProjectId = projectId;
        }

        public int ProjectId { get; }

        public class Handler : IRequestHandler<GetProjectErrorMessagesQuery, IReadOnlyList<ProjectErrorMessageDto>>
        {
            private static readonly string[] MessagesKeys =
            {
                "report.errorType.healthRiskNotFound",
                "report.errorType.dataCollectorUsedCollectionPointFormat",
                "report.errorType.collectionPointUsedDataCollectorFormat",
                "report.errorType.gateway",
                //?? Wrong reporting format: Reporting format cannot be used for human health risks
            };

            private readonly INyssContext _context;

            private readonly IStringsResourcesService _stringsResourcesService;

            public Handler(INyssContext context, IStringsResourcesService stringsResourcesService)
            {
                _context = context;
                _stringsResourcesService = stringsResourcesService;
            }

            public async Task<IReadOnlyList<ProjectErrorMessageDto>> Handle(GetProjectErrorMessagesQuery request, CancellationToken cancellationToken)
            {
                var project = await _context.Projects
                    .AsNoTracking()
                    .Include(x => x.NationalSociety)
                    .ThenInclude(x => x.ContentLanguage)
                    .SingleOrDefaultAsync(x => x.Id == request.ProjectId, cancellationToken);

                if (project == null)
                {
                    return new List<ProjectErrorMessageDto>();
                }

                var lang = project.NationalSociety.ContentLanguage.LanguageCode;

                var strings = await _stringsResourcesService.GetStrings(lang);

                var projectMessages = await _context.ProjectErrorMessages
                    .Where(x => x.ProjectId == request.ProjectId)
                    .ToListAsync(cancellationToken);

                return MessagesKeys.Select(key => new ProjectErrorMessageDto
                {
                    Key = key,
                    Message = projectMessages.SingleOrDefault(x => x.MessageKey == key)?.Message ?? strings.Get(key),
                }).ToArray();
            }
        }
    }
}
