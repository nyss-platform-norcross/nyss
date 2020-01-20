using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class GetDataCollectorResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Sex? Sex { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public int? BirthGroupDecade { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int VillageId { get; set; }

        public int DistrictId { get; set; }

        public int RegionId { get; set; }

        public int? ZoneId { get; set; }

        public int SupervisorId { get; set; }

        public FormDataDto FormData { get; set; }

        public int NationalSocietyId { get; set; }

        public int ProjectId { get; set; }

        public class FormDataDto
        {
            public IEnumerable<RegionResponseDto> Regions { get; set; }

            public IEnumerable<DistrictResponseDto> Districts { get; set; }

            public IEnumerable<VillageResponseDto> Villages { get; set; }

            public IEnumerable<ZoneResponseDto> Zones { get; set; }

            public IEnumerable<DataCollectorSupervisorResponseDto> Supervisors { get; set; }
        }
    }
}
