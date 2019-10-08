using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TimeZone { get; set; }

        public ProjectState State { get; set; }

        public virtual ContentLanguage ContentLanguage { get; set; }

        public virtual NationalSociety NationalSociety { get; set; }
    }
}
