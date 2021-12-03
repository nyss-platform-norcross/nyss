using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors.Commands
{
    public class SetDeployedStateCommand : IRequest<Result>
    {
        public IEnumerable<int> DataCollectorIds { get; set; }
        public bool Deployed { get; set; }

        public class Handler : IRequestHandler<SetDeployedStateCommand, Result>
        {
            private readonly INyssContext _nyssContext;
            private readonly IDateTimeProvider _dateTimeProvider;

            public Handler(INyssContext nyssContext, IDateTimeProvider dateTimeProvider)
            {
                _nyssContext = nyssContext;
                _dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result> Handle(SetDeployedStateCommand command, CancellationToken cancellationToken)
            {
                var dataCollectors = await _nyssContext.DataCollectors
                    .Include(dc => dc.DatesNotDeployed)
                    .Where(dc => command.DataCollectorIds.Contains(dc.Id))
                    .ToListAsync(cancellationToken);

                await SetDeployedState(dataCollectors, command.Deployed, cancellationToken);

                await _nyssContext.SaveChangesAsync(cancellationToken);

                return SuccessMessage(command.Deployed
                    ? ResultKey.DataCollector.SetToDeployedSuccess
                    : ResultKey.DataCollector.SetToNotDeployedSuccess);
            }

            private async Task SetDeployedState(List<DataCollector> dataCollectors, bool deployed, CancellationToken cancellationToken)
            {
                if (deployed)
                {
                    var deployedEndDate = _dateTimeProvider.UtcNow;
                    dataCollectors.ForEach(dc =>
                    {
                        dc.Deployed = true;
                        var deployedDateToUpdate = dc.DatesNotDeployed.FirstOrDefault(d => d.EndDate == null);
                        if (deployedDateToUpdate != null)
                        {
                            deployedDateToUpdate.EndDate = deployedEndDate;
                        }
                    });
                }
                else
                {
                    dataCollectors.ForEach(dc => dc.Deployed = false);
                    var datesNotDeployed = dataCollectors.Select(dc => new DataCollectorNotDeployed
                    {
                        DataCollectorId = dc.Id,
                        StartDate = _dateTimeProvider.UtcNow
                    });

                    await _nyssContext.DataCollectorNotDeployedDates.AddRangeAsync(datesNotDeployed, cancellationToken);
                }
            }
        }
    }
}
