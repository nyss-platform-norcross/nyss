using System.Collections.Generic;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Dto
{
    public class StructureResponseDto
    {
        public IEnumerable<StructureRegionDto> Regions { get; set; }

        public class StructureRegionDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public IEnumerable<StructureDistrictDto> Districts { get; set; }
        }

        public class StructureDistrictDto
        {
            public int Id { get; set; }

            public int RegionId { get; set; }

            public string Name { get; set; }

            public IEnumerable<StructureVillageDto> Villages { get; set; }
        }

        public class StructureVillageDto
        {
            public int Id { get; set; }

            public int DistrictId { get; set; }

            public string Name { get; set; }

            public IEnumerable<StructureZoneDto> Zones { get; set; }
        }

        public class StructureZoneDto
        {
            public int Id { get; set; }

            public int VillageId { get; set; }

            public string Name { get; set; }
        }
    }
}
