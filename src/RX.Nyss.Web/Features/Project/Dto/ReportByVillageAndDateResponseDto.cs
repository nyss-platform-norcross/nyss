using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ReportByVillageAndDateResponseDto
    {
        public IEnumerable<VillageDto> Villages { get; set; }

        public IEnumerable<string> AllPeriods { get; set; }

        public class VillageDto
        {
            public string Name { get; set; }

            public IEnumerable<PeriodDto> Periods { get; set; }
        }

        public class PeriodDto
        {
            public string Period { get; set; }

            public int Count { get; set; }
        }

        public static ReportByVillageAndDateResponseDto Empty() =>
            new ReportByVillageAndDateResponseDto { AllPeriods = new List<string>(), Villages = new List<VillageDto>() };
    }
}
