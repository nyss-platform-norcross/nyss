using System;
using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class AlertEventLog
    {
        public int Id { get; set; }
        public int AlertId { get; set; }
        public int AlertEventTypeId { get; set; }
        public int? AlertEventSubtypeId { get; set; }
        public int LoggedById { get; set; }
        public string Textfield { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual User LoggedBy { get; set; }
        public virtual Alert Alert { get; set; }
        public virtual AlertEventType AlertEventType { get; set; }
        public virtual AlertEventSubtype AlertEventSubtype { get; set; }
    }
}
