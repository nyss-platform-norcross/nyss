using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.DataCollectors.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class EditDataCollectorRequestDto
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

        public class Validator : AbstractValidator<EditDataCollectorRequestDto>
        {
            public Validator(IDataCollectorValidationService dataCollectorValidationService)
            {
                RuleFor(dc => dc.Id)
                    .GreaterThan(0);

                RuleFor(dc => dc.DataCollectorType)
                    .IsInEnum();

                RuleFor(dc => dc.Name)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(dc => dc.PhoneNumber)
                    .MaximumLength(20);

                RuleFor(dc => dc.PhoneNumber)
                    .MustAsync(async (model, phoneNumber, t) => !await dataCollectorValidationService.PhoneNumberExistsToOther(model.Id, phoneNumber))
                    .WithMessageKey(ResultKey.DataCollector.PhoneNumberAlreadyExists);

                RuleFor(dc => dc.AdditionalPhoneNumber)
                    .MaximumLength(20);

                RuleFor(dc => dc.SupervisorId)
                    .GreaterThan(0);

                RuleFor(dc => dc.SupervisorId)
                    .MustAsync(async (model, supervisorId, t) => await dataCollectorValidationService.IsAllowedToCreateForSupervisor(supervisorId))
                    .WithMessageKey(ResultKey.DataCollector.NotAllowedToSelectSupervisor);

                When(dc => dc.DataCollectorType == DataCollectorType.Human, () =>
                {
                    RuleFor(dc => dc.DisplayName).NotEmpty().MaximumLength(100);
                    RuleFor(dc => dc.Sex).IsInEnum();
                    RuleFor(dc => dc.BirthGroupDecade).GreaterThan(0).Must(x => x % 10 == 0);
                });

                RuleFor(dc => dc.Deployed)
                    .NotNull();

                RuleFor(dc => dc.Locations)
                    .NotNull()
                    .Must(locations => locations.Any());

                RuleForEach(dc => dc.Locations)
                    .Must(dcl => dcl.VillageId > 0
                        && dcl.Latitude >= -90 && dcl.Latitude <= 90
                        && dcl.Longitude >= -180 && dcl.Longitude <= 180);

                RuleForEach(dc => dc.Locations)
                    .MustAsync(async (model, location, t) => await dataCollectorValidationService.LocationHasDuplicateVillageAndZone(model.Id, location))
                    .WithMessageKey(ResultKey.DataCollector.DuplicateLocation);
            }
        }
    }
}
