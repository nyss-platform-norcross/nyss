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
    public class CreateDataCollectorCommand : IRequest<Result>
    {
        public int ProjectId { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public Sex? Sex { get; set; }

        public int? BirthGroupDecade { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string? AdditionalPhoneNumber { get; set; }

        public int SupervisorId { get; set; }

        public bool Deployed { get; set; }

        public bool LinkedToHeadSupervisor { get; set; }

        public IEnumerable<DataCollectorLocationRequestDto> Locations { get; set; } = Enumerable.Empty<DataCollectorLocationRequestDto>();

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
                    .Where(p => p.Id == command.ProjectId)
                    .Select(p => new
                    {
                        p.Id,
                        p.State,
                        NationalSocietyId = p.NationalSociety.Id
                    })
                    .SingleOrDefaultAsync(cancellationToken);

                if (project is null)
                {
                    return Error(ResultKey.DataCollector.CreateError);
                }

                if (project.State != ProjectState.Open)
                {
                    return Error(ResultKey.DataCollector.ProjectIsClosed);
                }

                SupervisorUser? supervisor = null;
                HeadSupervisorUser? headSupervisor = null;

                if (!command.LinkedToHeadSupervisor)
                {
                    supervisor = await _nyssContext.UserNationalSocieties
                        .FilterAvailableUsers()
                        .Where(u => u.User!.Id == command.SupervisorId
                            && u.User.Role == Role.Supervisor
                            && u.NationalSocietyId == project.NationalSocietyId)
                        .Select(u => (SupervisorUser)u.User!)
                        .SingleOrDefaultAsync(cancellationToken);

                    if (supervisor is null)
                    {
                        return Error(ResultKey.DataCollector.CreateError);
                    }
                }
                else
                {
                    headSupervisor = await _nyssContext.UserNationalSocieties
                        .FilterAvailableUsers()
                        .Where(u => u.User!.Id == command.SupervisorId
                            && u.User.Role == Role.HeadSupervisor
                            && u.NationalSocietyId == project.NationalSocietyId)
                        .Select(u => (HeadSupervisorUser)u.User!)
                        .SingleOrDefaultAsync(cancellationToken);

                    if (headSupervisor is null)
                    {
                        return Error(ResultKey.DataCollector.CreateError);
                    }
                }

                var villageIds = command.Locations.Select(l => l.VillageId).Distinct().ToList();
                var zoneIds = command.Locations
                    .Where(l => l.ZoneId.HasValue)
                    .Select(l => l.ZoneId!.Value)
                    .Distinct()
                    .ToList();

                var villages = await _nyssContext.Villages
                    .Where(v => villageIds.Contains(v.Id) && v.District!.Region.NationalSociety.Id == project.NationalSocietyId)
                    .ToDictionaryAsync(v => v.Id, cancellationToken);

                var zones = zoneIds.Any()
                    ? await _nyssContext.Zones
                        .Where(z => zoneIds.Contains(z.Id))
                        .ToDictionaryAsync(z => z.Id, cancellationToken)
                    : new Dictionary<int, Zone>();

                var missingVillages = villageIds.Where(id => !villages.ContainsKey(id)).ToList();
                if (missingVillages.Any())
                {
                    return Error(ResultKey.DataCollector.LocationError);
                }

                var missingZones = zoneIds.Where(id => !zones.ContainsKey(id)).ToList();
                if (missingZones.Any())
                {
                    return Error(ResultKey.DataCollector.LocationError);
                }

                var projectRef = new Project { Id = project.Id };
                _nyssContext.Projects.Attach(projectRef);

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
                    Project = projectRef,
                    CreatedAt = _dateTimeProvider.UtcNow,
                    IsInTrainingMode = true,
                    Deployed = command.Deployed,
                    DataCollectorLocations = command.Locations.Select(l => new DataCollectorLocation
                    {
                        Location = CreatePoint(l.Latitude, l.Longitude),
                        Village = villages[l.VillageId],
                        Zone = l.ZoneId.HasValue ? zones[l.ZoneId.Value] : null
                    }).ToList()
                };

                _nyssContext.DataCollectors.Add(dataCollector);
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
