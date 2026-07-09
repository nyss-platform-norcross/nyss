namespace RX.Nyss.Data.Models
{
    public class AlertEventSubtype
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AlertEventTypeId { get; set; }
        public virtual AlertEventType AlertEventType
        {
            get; set;
        }

    }
}
