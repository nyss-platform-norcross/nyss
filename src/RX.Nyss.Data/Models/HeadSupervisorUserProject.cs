namespace RX.Nyss.Data.Models
{
    public class HeadSupervisorUserProject
    {
        public int HeadSupervisorUserId { get; set; }
        public virtual HeadSupervisorUser HeadSupervisorUser { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}
