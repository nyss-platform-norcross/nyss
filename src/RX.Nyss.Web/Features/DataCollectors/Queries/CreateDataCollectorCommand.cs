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

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public class CreateDataCollectorCommand : IRequest<Result>
    {
        public int ProjectId { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Sex? Sex { get; set; }

        public int? BirthGroupDecade { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public int SupervisorId { get; set; }

        public bool Deployed { get; set; }

        public bool LinkedToHeadSupervisor { get; set; }

        public IEnumerable<DataCollectorLocationRequestDto> Locations { get; set; }

        public class Handler : IRequestHandler<CreateDataCollectorCommand, Result>
        {
            private readonly INyssContext _nyssContext;
            private readonly IDateTimeProvider _dateTimeProvider;

            public Handler(
                INyssContext nyssContext,
                IDateTimeProvider dateTimeProvider)
            {
                _nyssContext = nyssContext;
                _dateTimeProvider = dateTimeProvider;
            }

            public async Task<Result> Handle(CreateDataCollectorCommand command, CancellationToken cancellationToken)
            {
                var project = await _nyssContext.Projects
                    .Include(p => p.NationalSociety)
                    .SingleAsync(p => p.Id == command.ProjectId, cancellationToken);

                if (project.State != ProjectState.Open)
                {
                    return Error(ResultKey.DataCollector.ProjectIsClosed);
                }

                var nationalSocietyId = project.NationalSociety.Id;

                var supervisor = !command.LinkedToHeadSupervisor
                    ? await _nyssContext.UserNationalSocieties
                        .FilterAvailableUsers()
                        .Where(u => u.User.Id == command.SupervisorId && u.User.Role == Role.Supervisor && u.NationalSocietyId == nationalSocietyId)
                        .Select(u => (SupervisorUser)u.User)
                        .SingleAsync(cancellationToken)
                    : null;

                var headSupervisor = command.LinkedToHeadSupervisor
                    ? await _nyssContext.UserNationalSocieties
                        .FilterAvailableUsers()
                        .Where(u => u.User.Id == command.SupervisorId && u.User.Role == Role.HeadSupervisor && u.NationalSocietyId == nationalSocietyId)
                        .Select(u => (HeadSupervisorUser)u.User)
                        .SingleAsync(cancellationToken)
                    : null;

                var dataCollector = new DataCollector
                {
                    Name = command.Name,
                    DisplayName = command.DisplayName,
                    PhoneNumber = command.PhoneNumber,
                    AdditionalPhoneNumber = command.AdditionalPhoneNumber,
                    BirthGroupDecade = command.BirthGroupDecade,
                    Sex = command.Sex,
                    DataCollectorType = command.DataCollectorType,
                    Supervisor = supervisor,
                    HeadSupervisor = headSupervisor,
                    Project = project,
                    CreatedAt = _dateTimeProvider.UtcNow,
                    IsInTrainingMode = true,
                    Deployed = command.Deployed,
                    DataCollectorLocations = command.Locations.Select(l => new DataCollectorLocation
                    {
                        Location = CreatePoint(l.Latitude, l.Longitude),
                        Village = _nyssContext.Villages
                            .Single(v => v.Id == l.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId),
                        Zone = l.ZoneId != null
                            ? _nyssContext.Zones.Single(z => z.Id == l.ZoneId.Value)
                            : null
                    }).ToList()
                };

                await _nyssContext.AddAsync(dataCollector, cancellationToken);
                await _nyssContext.SaveChangesAsync(cancellationToken);
                return Success(ResultKey.DataCollector.CreateSuccess);
            }

            private static Point CreatePoint(double latitude, double longitude)
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(SpatialReferenceSystemIdentifier.Wgs84);
                return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            }
        }
    }
}
