using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class District
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual Region Region { get; set; }

        public virtual ICollection<Village> Villages { get; set; }

        public virtual EidsrOrganisationUnits EidsrOrganisationUnits { get; set; }
    }
}
