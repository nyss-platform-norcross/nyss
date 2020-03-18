using System;
using System.Linq;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Reports;

namespace RX.Nyss.Web.Services.GeographicalCoverage
{
    public interface IGeographicalCoverageService
    {
        IQueryable<int> GetNumberOfVillagesByProject(int projectId, ReportsFilter filters);
        IQueryable<int> GetNumberOfDistrictsByProject(int projectId, ReportsFilter filters);
        IQueryable<int> GetNumberOfVillagesByNationalSociety(int nationalSocietyId, ReportsFilter filters);
        IQueryable<int> GetNumberOfDistrictsByNationalSociety(int nationalSocietyId, ReportsFilter filters);
    }

    public class GeographicalCoverageService : IGeographicalCoverageService
    {
        private readonly INyssContext _nyssContext;

        public GeographicalCoverageService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public IQueryable<int> GetNumberOfVillagesByProject(int projectId, ReportsFilter filters) =>
            GetDataCollectorsByProject(projectId, filters)
                .Select(dc => dc.Village.Id).Distinct();

        public IQueryable<int> GetNumberOfDistrictsByProject(int projectId, ReportsFilter filters) =>
            GetDataCollectorsByProject(projectId, filters)
                .Select(dc => dc.Village.District.Id).Distinct();

        public IQueryable<int> GetNumberOfVillagesByNationalSociety(int nationalSocietyId, ReportsFilter filters) =>
            GetDataCollectorsByNationalSociety(nationalSocietyId, filters)
                .Select(dc => dc.Village.Id).Distinct();

        public IQueryable<int> GetNumberOfDistrictsByNationalSociety(int nationalSocietyId, ReportsFilter filters) =>
            GetDataCollectorsByNationalSociety(nationalSocietyId, filters)
                .Select(dc => dc.Village.District.Id).Distinct();

        private IQueryable<DataCollector> GetDataCollectorsByProject(int projectId, ReportsFilter filters) => 
            _nyssContext.DataCollectors
                .FilterByProject(projectId)
                .FilterByArea(filters.Area)
                .FilterOnlyNotDeletedBefore(filters.StartDate)
                .FilterByType(filters.DataCollectorType);

        private IQueryable<DataCollector> GetDataCollectorsByNationalSociety(int nationalSocietyId, ReportsFilter filters) => 
            _nyssContext.DataCollectors
                .FilterByNationalSociety(nationalSocietyId)
                .FilterByArea(filters.Area)
                .FilterOnlyNotDeletedBefore(filters.StartDate)
                .FilterByType(filters.DataCollectorType);
    }
}