using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class FiltersRequestDto
    {
        public AreaDto Area { get; set; }

        public SexDto? Sex { get; set; }

        public int? SupervisorId { get; set; }

        public TrainingStatusDto? TrainingStatus { get; set; }
    }
}
