using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TimeZone { get; set; }

        public ProjectState State { get; set; }

        public ContentLanguage ContentLanguage { get; set; }

        public NationalSociety NationalSociety { get; set; }
    }
}
