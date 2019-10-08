namespace RX.Nyss.Data.Models
{
    public class SupervisorUser : User
    {
        public string Sex { get; set; }

        public NationalSociety NationalSociety { get; set; }

        public Village Village { get; set; }

        public Zone Zone { get; set; }

        public DataManagerUser DataManagerUser { get; set; }
    }
}
