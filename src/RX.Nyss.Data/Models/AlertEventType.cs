using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class AlertEventType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<AlertEventSubtype> AlertEventSubtype { get; set; }
    }
}
