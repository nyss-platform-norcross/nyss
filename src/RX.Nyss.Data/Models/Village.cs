using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class Village
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public virtual District? District { get; set; }

        public virtual ICollection<Zone>? Zones { get; set; }
    }
}
