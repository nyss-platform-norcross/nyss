using System.Collections.Generic;
using FluentValidation;

namespace RX.Nyss.Web.Features.Common.Dto
{
    public class AreaDto
    {
        public IEnumerable<int> RegionIds { get; set; }

        public IEnumerable<int> DistrictIds { get; set; }

        public IEnumerable<int> VillageIds { get; set; }

        public IEnumerable<int> ZoneIds { get; set; }

        public bool IncludeUnknownLocation { get; set; }

        public class Validator : AbstractValidator<AreaDto>
        {
            public Validator()
            {
                RuleForEach(a => a.RegionIds).Must(id => id > 0);
                RuleForEach(a => a.DistrictIds).Must(id => id > 0);
                RuleForEach(a => a.VillageIds).Must(id => id > 0);
                RuleForEach(a => a.ZoneIds).Must(id => id > 0);
            }
        }
    }
}
