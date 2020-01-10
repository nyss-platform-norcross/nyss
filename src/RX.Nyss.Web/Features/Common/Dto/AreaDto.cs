namespace RX.Nyss.Web.Features.Common.Dto
{
    public class AreaDto
    {
        public int Id { get; set; }

        public AreaType Type { get; set; }
        
        public enum AreaType
        {
            Region,
            District,
            Village,
            Zone
        }
    }
}
