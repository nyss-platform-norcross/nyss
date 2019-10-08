namespace RX.Nyss.Data.Models
{
    public class NationalSociety
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsArchived { get; set; }

        public string RegionCustomName { get; set; }

        public string DistrictCustomName { get; set; }

        public string VillageCustomName { get; set; }

        public string ZoneCustomName { get; set; }

        public ContentLanguage ContentLanguage { get; set; }
    }
}
