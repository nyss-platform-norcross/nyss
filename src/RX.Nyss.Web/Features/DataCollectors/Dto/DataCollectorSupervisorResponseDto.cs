using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorSupervisorResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public Role Role { get; set; }
    }
}
