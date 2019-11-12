namespace RX.Nyss.Data.Models
{
    public class SupervisorUserProject
    {
        public int SupervisorUserId { get; set; }
        public virtual SupervisorUser SupervisorUser { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}
