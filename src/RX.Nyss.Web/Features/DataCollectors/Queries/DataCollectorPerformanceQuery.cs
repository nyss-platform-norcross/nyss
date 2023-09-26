using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public class DataCollectorPerformanceQuery : IRequest<Result<DataCollectorPerformanceResponseDto>>
    {
        public int ProjectId { get; }

        public DataCollectorPerformanceFiltersRequestDto Filter { get; }

        public DataCollectorPerformanceQuery(int projectId, DataCollectorPerformanceFiltersRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public class Handler : IRequestHandler<DataCollectorPerformanceQuery, Result<DataCollectorPerformanceResponseDto>>
        {
            private readonly INyssContext _nyssContext;
            private readonly IDataCollectorService _dataCollectorService;
            private readonly IDataCollectorPerformanceService _dataCollectorPerformanceService;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly INyssWebConfig _config;

            public Handler(
                INyssContext nyssContext,
                IDataCollectorService dataCollectorService,
                IDataCollectorPerformanceService dataCollectorPerformanceService,
                IDateTimeProvider dateTimeProvider,
                INyssWebConfig config)
            {
                _nyssContext = nyssContext;
                _dataCollectorService = dataCollectorService;
                _dataCollectorPerformanceService = dataCollectorPerformanceService;
                _dateTimeProvider = dateTimeProvider;
                _config = config;
            }


            public async Task<Result<DataCollectorPerformanceResponseDto>> Handle(DataCollectorPerformanceQuery request, CancellationToken cancellationToken)
            {
                var epiWeekStartDay = await _nyssContext.Projects
                    .Where(p => p.Id == request.ProjectId)
                    .Select(p => p.NationalSociety.EpiWeekStartDay)
                    .SingleAsync(cancellationToken);

                var dataCollectors = (await _dataCollectorService.GetDataCollectorsForCurrentUserInProject(request.ProjectId))
                    .Include(dc => dc.DataCollectorLocations)
                    .Include(dc => dc.DatesNotDeployed)
                    .FilterOnlyNotDeleted()
                    .FilterByArea(request.Filter.Locations)
                    .FilterByName(request.Filter.Name)
                    .FilterBySupervisor(request.Filter.SupervisorId)
                    .FilterByTrainingMode(request.Filter.TrainingStatus);

                var currentDate = _dateTimeProvider.UtcNow;
                var fromEpiDate = _dateTimeProvider.GetEpiDate(currentDate.AddDays(-8 * 7), epiWeekStartDay);
                var toEpiDate = _dateTimeProvider.GetEpiDate(currentDate.AddDays(-7), epiWeekStartDay);
                var fromDate = _dateTimeProvider.GetFirstDateOfEpiWeek(fromEpiDate.EpiYear, fromEpiDate.EpiWeek, epiWeekStartDay);
                var previousEpiWeekDate = _dateTimeProvider.GetFirstDateOfEpiWeek(toEpiDate.EpiYear, toEpiDate.EpiWeek, epiWeekStartDay);
                var rowsPerPage = _config.PaginationRowsPerPage;
                var totalRows = await dataCollectors.CountAsync(cancellationToken);
                var epiDateRange = _dateTimeProvider.GetEpiDateRange(fromDate, previousEpiWeekDate, epiWeekStartDay).ToList();

                var dcIsTraining = request.Filter.TrainingStatus == TrainingStatusDto.InTraining;

                var dataCollectorsWithReportsData = await _dataCollectorPerformanceService.GetDataCollectorsWithReportData(dataCollectors, fromDate, currentDate, cancellationToken, dcIsTraining);

                var dataCollectorCompleteness = _dataCollectorPerformanceService.GetDataCollectorCompleteness(dataCollectorsWithReportsData, epiDateRange, epiWeekStartDay)
                    .Reverse()
                    .ToList();

                var paginatedDataCollectorsWithReportsData = dataCollectorsWithReportsData
                    .Page(request.Filter.PageNumber, rowsPerPage);

                var dataCollectorPerformances = _dataCollectorPerformanceService.GetDataCollectorPerformance(paginatedDataCollectorsWithReportsData, currentDate, epiDateRange, epiWeekStartDay)
                    .AsPaginatedList(request.Filter.PageNumber, totalRows, rowsPerPage);

                var dataCollectorPerformanceDto = new DataCollectorPerformanceResponseDto
                {
                    Completeness = dataCollectorCompleteness,
                    Performance = dataCollectorPerformances,
                    EpiDateRange = epiDateRange.AsEnumerable().Reverse().ToList()
                };

                return Success(dataCollectorPerformanceDto);
            }
        }
    }
}
