using FluentValidation;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorLocationRequestDto
    {
        public int? Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int VillageId { get; set; }
        public int? ZoneId { get; set; }
    }
}
