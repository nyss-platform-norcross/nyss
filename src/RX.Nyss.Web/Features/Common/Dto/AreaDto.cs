using System.Collections.Generic;
using FluentValidation;

namespace RX.Nyss.Web.Features.Common.Dto
{
    public class AreaDto
    {
        public IEnumerable<int> RegionIds { get; set; } = new List<int>();

        public IEnumerable<int> DistrictIds { get; set; } = new List<int>();

        public IEnumerable<int> VillageIds { get; set; } = new List<int>();

        public IEnumerable<int> ZoneIds { get; set; } = new List<int>();

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
