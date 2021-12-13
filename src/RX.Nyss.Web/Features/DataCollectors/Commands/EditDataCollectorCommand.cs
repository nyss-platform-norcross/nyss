using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Utils;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors.Commands
{
    public class EditDataCollectorCommand : IRequest<Result>
    {
        public int Id { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Sex? Sex { get; set; }

        public int? BirthGroupDecade { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public int SupervisorId { get; set; }

        public bool Deployed { get; set; }

        public IEnumerable<DataCollectorLocationRequestDto> Locations { get; set; }

        public bool LinkedToHeadSupervisor { get; set; }

        public class Handler : IRequestHandler<EditDataCollectorCommand, Result>
        {
            private readonly INyssContext _nyssContext;
            private readonly IDateTimeProvider _dateTimeProvider;

            public Handler(INyssContext nyssContext, IDateTimeProvider dateTimeProvider)
            {
                _nyssContext = nyssContext;
                _dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result> Handle(EditDataCollectorCommand request, CancellationToken cancellationToken)
            {
                var dataCollector = await _nyssContext.DataCollectors
                    .Include(dc => dc.Project)
                    .ThenInclude(x => x.NationalSociety)
                    .Include(dc => dc.Supervisor)
                    .Include(dc => dc.DataCollectorLocations)
                    .Include(dc => dc.DatesNotDeployed)
                    .SingleAsync(dc => dc.Id == request.Id, cancellationToken);

                if (dataCollector.Project.State != ProjectState.Open)
                {
                    return Error(ResultKey.DataCollector.ProjectIsClosed);
                }

                var nationalSocietyId = dataCollector.Project.NationalSociety.Id;

                var supervisor = !request.LinkedToHeadSupervisor
                    ? await _nyssContext.UserNationalSocieties
                        .FilterAvailableUsers()
                        .Where(u => u.User.Id == request.SupervisorId && u.User.Role == Role.Supervisor && u.NationalSocietyId == nationalSocietyId)
                        .Select(u => (SupervisorUser)u.User)
                        .SingleAsync(cancellationToken)
                    : null;

                var headSupervisor = request.LinkedToHeadSupervisor
                    ? await _nyssContext.UserNationalSocieties
                        .FilterAvailableUsers()
                        .Where(u => u.User.Id == request.SupervisorId && u.User.Role == Role.HeadSupervisor && u.NationalSocietyId == nationalSocietyId)
                        .Select(u => (HeadSupervisorUser)u.User)
                        .SingleAsync(cancellationToken)
                    : null;

                dataCollector.Name = request.Name;
                dataCollector.DisplayName = request.DisplayName;
                dataCollector.PhoneNumber = request.PhoneNumber;
                dataCollector.AdditionalPhoneNumber = request.AdditionalPhoneNumber;
                dataCollector.BirthGroupDecade = request.BirthGroupDecade;
                dataCollector.Sex = request.Sex;
                dataCollector.Supervisor = supervisor;
                dataCollector.HeadSupervisor = headSupervisor;
                dataCollector.Deployed = request.Deployed;

                if (request.Deployed)
                {
                    UpdateNotDeployedDate(dataCollector);
                }
                else
                {
                    await AddNotDeployedDate(dataCollector, cancellationToken);
                }

                dataCollector.DataCollectorLocations = await EditDataCollectorLocations(dataCollector.DataCollectorLocations, request.Locations, nationalSocietyId);

                await _nyssContext.SaveChangesAsync(cancellationToken);
                return SuccessMessage(ResultKey.DataCollector.EditSuccess);
            }


            private async Task<List<DataCollectorLocation>> EditDataCollectorLocations(ICollection<DataCollectorLocation> dataCollectorLocations,
                IEnumerable<DataCollectorLocationRequestDto> dataCollectorLocationRequestDtos, int nationalSocietyId)
            {
                var newDataCollectorLocations = new List<DataCollectorLocation>();
                foreach (var dto in dataCollectorLocationRequestDtos)
                {
                    var dcl = dto.Id.HasValue
                        ? dataCollectorLocations.First(l => l.Id == dto.Id)
                        : new DataCollectorLocation();

                    dcl.Location = CreatePoint(dto.Latitude, dto.Longitude);
                    dcl.Village = await _nyssContext.Villages.SingleAsync(v => v.Id == dto.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId);
                    dcl.Zone = await _nyssContext.Zones.SingleOrDefaultAsync(z => z.Id == dto.ZoneId);

                    newDataCollectorLocations.Add(dcl);
                }

                return newDataCollectorLocations;
            }

            private static Point CreatePoint(double latitude, double longitude)
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(SpatialReferenceSystemIdentifier.Wgs84);
                return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            }

            private void UpdateNotDeployedDate(DataCollector dataCollector)
            {
                var deployedDateToUpdate = dataCollector.DatesNotDeployed.FirstOrDefault(d => d.EndDate == null);
                if (deployedDateToUpdate != null)
                {
                    deployedDateToUpdate.EndDate = _dateTimeProvider.UtcNow;
                }
            }

            private async Task AddNotDeployedDate(DataCollector dataCollector, CancellationToken cancellationToken)
            {
                var dateNotDeployed = new DataCollectorNotDeployed
                {
                    DataCollectorId = dataCollector.Id,
                    StartDate = _dateTimeProvider.UtcNow
                };

                await _nyssContext.DataCollectorNotDeployedDates.AddAsync(dateNotDeployed, cancellationToken);
            }
        }
    }
}
