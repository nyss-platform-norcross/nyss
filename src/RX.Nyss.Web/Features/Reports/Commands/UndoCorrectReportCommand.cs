using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports.Commands
{
    public class UndoCorrectReportCommand : IRequest<Result>
    {
        public UndoCorrectReportCommand(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public class Handler : IRequestHandler<UndoCorrectReportCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            public Handler(
                INyssContext nyssContext,
                IAuthorizationService authorizationService)
            {
                _nyssContext = nyssContext;
                _authorizationService = authorizationService;
            }

            public async Task<Result> Handle(UndoCorrectReportCommand request, CancellationToken cancellationToken)
            {
                var report = await _nyssContext.RawReports
                    .SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

                if (report == null)
                {
                    return Result.Error("NotFound");
                }

                report.MarkedAsCorrectedAtUtc = null;
                report.MarkedAsCorrectedBy = _authorizationService.GetCurrentUserName();

                await _nyssContext.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }
    }
}
