namespace RX.Nyss.Data.Models
{
    public class UserNationalSociety
    {
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public int NationalSocietyId { get; set; }
        public virtual NationalSociety? NationalSociety { get; set; }

        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }
    }
}
