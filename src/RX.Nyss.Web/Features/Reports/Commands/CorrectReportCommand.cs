using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;

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
            public async Task<Result> Handle(CorrectReportCommand request, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;

                return Result.Success();
            }
        }
    }
}
