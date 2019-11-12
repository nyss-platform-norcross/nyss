namespace RX.Nyss.Data.Models
{
    public class SupervisorUser : User
    {
        public string Sex { get; set; }

        public virtual Village Village { get; set; }

        public virtual Zone Zone { get; set; }

        public virtual ManagerUser ManagerUser { get; set; }
    }
}
