using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class Region
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual NationalSociety NationalSociety { get; set; }

        public virtual ICollection<District> Districts { get; set; }
    }
}
