namespace RX.Nyss.Web.Features.Common.Dto
{
    public class AreaDto
    {
        public int Id { get; set; }

        public AreaTypeDto Type { get; set; }
        
        public enum AreaTypeDto
        {
            Region,
            District,
            Village,
            Zone
        }
    }
}
