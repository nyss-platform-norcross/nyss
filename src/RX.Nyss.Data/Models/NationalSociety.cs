using System;
using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class NationalSociety
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public bool IsArchived { get; set; }

        public string RegionCustomName { get; set; }

        public string DistrictCustomName { get; set; }

        public string VillageCustomName { get; set; }

        public string ZoneCustomName { get; set; }

        public virtual ContentLanguage ContentLanguage { get; set; }

        public virtual Country Country { get; set; }

        public virtual User HeadManager { get; set; }

        public virtual User PendingHeadManager { get; set; }

        public virtual ICollection<UserNationalSociety> NationalSocietyUsers { get; set; }

        public virtual ICollection<RawReport> RawReports { get; set; }
    }
}
