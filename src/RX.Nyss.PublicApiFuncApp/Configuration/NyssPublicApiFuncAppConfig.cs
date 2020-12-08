namespace RX.Nyss.PublicApiFuncApp.Configuration
{
    public interface IConfig
    {
        string ReleaseName { get; set; }
    }

    public class NyssPublicApiFuncAppConfig : IConfig
    {
        public string ReleaseName { get; set; }
    }
}
