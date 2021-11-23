using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports.Commands
{
    public class CorrectReportCommand : IRequest<Result>
    {
        public CorrectReportCommand(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public class Handler : IRequestHandler<CorrectReportCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly IDateTimeProvider _dateTimeProvider;

            private readonly IAuthorizationService _authorizationService;

            public Handler(
                INyssContext nyssContext,
                IDateTimeProvider dateTimeProvider,
                IAuthorizationService authorizationService)
            {
                _nyssContext = nyssContext;
                _dateTimeProvider = dateTimeProvider;
                _authorizationService = authorizationService;
            }

            public async Task<Result> Handle(CorrectReportCommand request, CancellationToken cancellationToken)
            {
                var report = await _nyssContext.RawReports
                    .SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

                if (report == null)
                {
                    return Result.Error("NotFound");
                }

                report.MarkedAsCorrectedAtUtc = _dateTimeProvider.UtcNow;
                report.MarkedAsCorrectedBy = _authorizationService.GetCurrentUserName();

                await _nyssContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
    }
}
