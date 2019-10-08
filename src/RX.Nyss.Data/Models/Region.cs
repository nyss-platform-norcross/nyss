namespace RX.Nyss.Data.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public NationalSociety NationalSociety{ get; set; }
    }
}
