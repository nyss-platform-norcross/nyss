using System;

namespace RX.Nyss.Data.Models
{
    public class ManagerUser : User
    {
        public bool IsDataOwner { get; set; }

        public bool HasConsented { get; set; }

        public DateTime? ConsentedAt { get; set; }
    }
}
