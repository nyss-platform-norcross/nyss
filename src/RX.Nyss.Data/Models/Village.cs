using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class Village
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public District District { get; set; }
    }
}
