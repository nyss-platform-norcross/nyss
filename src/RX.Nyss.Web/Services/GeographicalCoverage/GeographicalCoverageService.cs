using System;
using System.Linq;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Extensions;

namespace RX.Nyss.Web.Services.GeographicalCoverage
{
    public interface IGeographicalCoverageService
    {
        IQueryable<int> GetNumberOfVillagesByProject(int projectId, DateTime startDate);
        IQueryable<int> GetNumberOfDistrictsByProject(int projectId, DateTime startDate);
        IQueryable<int> GetNumberOfVillagesByNationalSociety(int nationalSocietyId, DateTime startDate);
        IQueryable<int> GetNumberOfDistrictsByNationalSociety(int nationalSocietyId, DateTime startDate);
    }

    public class GeographicalCoverageService : IGeographicalCoverageService
    {
        private readonly INyssContext _nyssContext;

        public GeographicalCoverageService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public IQueryable<int> GetNumberOfVillagesByProject(int projectId, DateTime startDate) =>
            GetDataCollectorsByProject(projectId, startDate)
                .Select(dc => dc.Village.Id).Distinct();

        public IQueryable<int> GetNumberOfDistrictsByProject(int projectId, DateTime startDate) =>
            GetDataCollectorsByProject(projectId, startDate)
                .Select(dc => dc.Village.District.Id).Distinct();

        public IQueryable<int> GetNumberOfVillagesByNationalSociety(int nationalSocietyId, DateTime startDate) =>
            GetDataCollectorsByNationalSociety(nationalSocietyId, startDate)
                .Select(dc => dc.Village.Id).Distinct();

        public IQueryable<int> GetNumberOfDistrictsByNationalSociety(int nationalSocietyId, DateTime startDate) =>
            GetDataCollectorsByNationalSociety(nationalSocietyId, startDate)
                .Select(dc => dc.Village.District.Id).Distinct();

        private IQueryable<DataCollector> GetDataCollectorsByProject(int projectId, DateTime startDate) => 
            _nyssContext.DataCollectors
                .FilterByProject(projectId)
                .FilterOnlyNotDeletedBefore(startDate);

        private IQueryable<DataCollector> GetDataCollectorsByNationalSociety(int nationalSocietyId, DateTime startDate) => 
            _nyssContext.DataCollectors
                .FilterByNationalSociety(nationalSocietyId)
                .FilterOnlyNotDeletedBefore(startDate);
    }
}