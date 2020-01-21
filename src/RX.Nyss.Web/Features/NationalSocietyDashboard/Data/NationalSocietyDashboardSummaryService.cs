using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Data
{
    public interface INationalSocietyDashboardSummaryService
    {
        Task<NationalSocietySummaryResponseDto> GetSummaryData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);
    }

    public class NationalSocietyDashboardSummaryService : INationalSocietyDashboardSummaryService
    {
        private readonly INyssContext _nyssContext;

        public NationalSocietyDashboardSummaryService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public Task<NationalSocietySummaryResponseDto> GetSummaryData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto)
        {
            var assignedRawReports = NationalSocietyDashboardQueries.GetAssignedRawReports(_nyssContext.RawReports, nationalSocietyId, filtersDto);
            var validReports = NationalSocietyDashboardQueries.GetValidReports(_nyssContext.RawReports, nationalSocietyId, filtersDto);
            var dataCollectionPointReports = validReports.Where(r => r.DataCollectionPointCase != null);

            return _nyssContext.Projects
                .Where(ph => ph.NationalSocietyId == nationalSocietyId)
                .Select(ph => new
                {
                    allDataCollectorCount = AllDataCollectorCount(filtersDto, nationalSocietyId),
                    activeDataCollectorCount = assignedRawReports.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new NationalSocietySummaryResponseDto
                {
                    ReportCount = validReports.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.activeDataCollectorCount,
                    InactiveDataCollectorCount = data.allDataCollectorCount - data.activeDataCollectorCount,
                    DataCollectionPointSummary = new NationalSocietyDataCollectionPointsSummaryResponse
                    {
                        FromOtherVillagesCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.FromOtherVillagesCount ?? 0),
                        ReferredToHospitalCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.ReferredCount ?? 0),
                        DeathCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.DeathCount ?? 0),
                    }
                })
                .FirstOrDefaultAsync();
        }

        private int AllDataCollectorCount(NationalSocietyDashboardFiltersRequestDto filtersDto, int nationalSocietyId) =>
            _nyssContext.DataCollectors
                .FilterByArea(filtersDto.Area)
                .FilterByType(NationalSocietyDashboardQueries.MapToDataCollectorType(filtersDto.ReportsType))
                .FilterByNationalSociety(nationalSocietyId)
                .FilterByTrainingMode(filtersDto.IsTraining)
                .FilterOnlyNotDeletedBefore(filtersDto.StartDate)
                .Count();
    }
}
