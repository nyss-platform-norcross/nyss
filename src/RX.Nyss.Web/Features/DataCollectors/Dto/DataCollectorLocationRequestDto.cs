using FluentValidation;
using Microsoft.CodeAnalysis;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.DataCollectors.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorLocationRequestDto
    {
        public int? Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int VillageId { get; set; }
        public int? ZoneId { get; set; }

        public class Validator : AbstractValidator<DataCollectorLocationRequestDto>
        {
            public Validator(int dataCollectorId, IDataCollectorValidationService dataCollectorValidationService)
            {
                RuleFor(l => l.Latitude).InclusiveBetween(-90, 90);

                RuleFor(l => l.Longitude).InclusiveBetween(-180, 180);

                RuleFor(l => l.VillageId).GreaterThan(0);

                RuleFor(l => l.ZoneId).GreaterThan(0).When(l => l.ZoneId.HasValue);

                RuleFor(l => l.VillageId)
                    .MustAsync(async (model, villageId, t) => !await dataCollectorValidationService.LocationHasDuplicateVillage(dataCollectorId, model))
                    .WithMessageKey(ResultKey.DataCollector.DuplicateLocation);

                RuleFor(l => l.ZoneId)
                    .MustAsync(async (model, zoneId, t) => !zoneId.HasValue || !await dataCollectorValidationService.LocationHasDuplicateZone(dataCollectorId, model))
                    .WithMessageKey(ResultKey.DataCollector.DuplicateLocation);
            }
        }
    }
}
