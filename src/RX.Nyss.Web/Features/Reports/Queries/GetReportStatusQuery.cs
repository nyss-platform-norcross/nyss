using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public class GetReportStatusQuery : IRequest<ReportStatusDto>
    {
        public string Timestamp { get; set; }

        public int DataCollectorId { get; set; }

        public class Handler : IRequestHandler<GetReportStatusQuery, ReportStatusDto>
        {
            private readonly INyssContext _context;

            private readonly IStringsResourcesService _stringsResourcesService;

            private readonly IAuthorizationService _authorizationService;

            public Handler(
                INyssContext context,
                IStringsResourcesService stringsResourcesService,
                IAuthorizationService authorizationService)
            {
                _context = context;
                _stringsResourcesService = stringsResourcesService;
                _authorizationService = authorizationService;
            }

            public async Task<ReportStatusDto> Handle(GetReportStatusQuery request, CancellationToken cancellationToken)
            {
                var rawReport = await _context.RawReports
                    .AsNoTracking()
                    .Include(r => r.DataCollector)
                    .FirstOrDefaultAsync(r => r.Timestamp == request.Timestamp && r.DataCollector.Id == request.DataCollectorId, cancellationToken);

                if (rawReport == null)
                {
                    return null;
                }

                if (rawReport.ErrorType == null)
                {
                    return new ReportStatusDto();
                }

                var smsErrorKey = rawReport.ErrorType.Value.ToSmsErrorKey();

                var projectId = await _context.DataCollectors
                    .Where(d => d.Id == request.DataCollectorId)
                    .Select(d => d.Project.Id)
                    .SingleAsync(cancellationToken);
                var projectErrorMessage = await _context.ProjectErrorMessages
                    .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.MessageKey == smsErrorKey, cancellationToken);

                if (!string.IsNullOrWhiteSpace(projectErrorMessage?.Message))
                {
                    return new ReportStatusDto
                    {
                        FeedbackMessage = projectErrorMessage.Message,
                    };
                }

                var user = await _authorizationService.GetCurrentUser();
                var langCode = user.ApplicationLanguage?.LanguageCode ?? "en";

                var smsContents = (await _stringsResourcesService.GetSmsContentResources(langCode)).Value;

                return new ReportStatusDto
                {
                    FeedbackMessage = smsContents.TryGetValue(smsErrorKey, out var msg) ? msg : smsErrorKey,
                };
            }
        }
    }
}
