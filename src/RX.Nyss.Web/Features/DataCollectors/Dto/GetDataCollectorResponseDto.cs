using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class GetDataCollectorResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Sex? Sex { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public int? BirthGroupDecade { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string? AdditionalPhoneNumber { get; set; }

        public IEnumerable<DataCollectorLocationDto> Locations { get; set; } = new List<DataCollectorLocationDto>();
        public int SupervisorId { get; set; }

        public FormDataDto FormData { get; set; } = new FormDataDto();

        public int NationalSocietyId { get; set; }

        public string NationalSocietyCountryCode { get; set; }  = string.Empty;

        public int ProjectId { get; set; }

        public bool Deployed { get; set; }

        public class DataCollectorLocationDto
        {
            public int Id { get; set; }
            public double Latitude { get; set; }

            public double Longitude { get; set; }

            public int VillageId { get; set; }

            public int DistrictId { get; set; }

            public int RegionId { get; set; }

            public int? ZoneId { get; set; }
            public DataCollectorLocationFormDataDto InitialFormData { get; set; } = new DataCollectorLocationFormDataDto();
        }

        public class DataCollectorLocationFormDataDto
        {
            public IEnumerable<DistrictResponseDto> Districts { get; set; } = new List<DistrictResponseDto>();

            public IEnumerable<VillageResponseDto> Villages { get; set; } = new List<VillageResponseDto>();

            public IEnumerable<ZoneResponseDto>? Zones { get; set; }
        }

        public class FormDataDto
        {
            public IEnumerable<RegionResponseDto> Regions { get; set; } = new List<RegionResponseDto>();
            public IEnumerable<DataCollectorSupervisorResponseDto> Supervisors { get; set; } = new List<DataCollectorSupervisorResponseDto>();
        }
    }
}
