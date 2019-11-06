using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.DataConsumer.Dto
{
    public class GetDataConsumerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public Role Role { get; set; }
    }
}
