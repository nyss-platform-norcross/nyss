using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class DataCollector
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public DataCollectorType DataCollectorType { get; set; }

        public string? DisplayName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? AdditionalPhoneNumber { get; set; }

        public Sex? Sex { get; set; }

        public int? BirthGroupDecade { get; set; }

        public bool IsInTrainingMode { get; set; }


        public DateTime CreatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool Deployed { get; set; }

        public virtual SupervisorUser? Supervisor { get; set; }

        public virtual HeadSupervisorUser? HeadSupervisor { get; set; }

        public virtual Project? Project { get; set; }

        public ICollection<RawReport>? RawReports { get; set; }

        public ICollection<Report>? Reports { get; set; }

        public ICollection<DataCollectorLocation>? DataCollectorLocations { get; set; }

        public ICollection<DataCollectorNotDeployed>? DatesNotDeployed { get; set; }
    }
}
