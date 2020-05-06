using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NationalSocietyId { get; set; }
        public virtual NationalSociety NationalSociety { get; set; }
        public virtual ICollection<UserNationalSociety> NationalSocietyUsers { get; set; }
    }
}
