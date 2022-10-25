using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HealthRiskSuspectedDiseaseMap : IEntityTypeConfiguration<HealthRiskSuspectedDisease>
    {
        public void Configure(EntityTypeBuilder<HealthRiskSuspectedDisease> builder)
        {
            builder.HasKey(x => x.HealthRiskId);
            builder.HasKey(x => x.SuspectedDiseaseId);

            builder.HasOne(x => x.HealthRisk)
                .WithMany(x => x.HealthRiskSuspectedDiseases)
                .HasForeignKey(phr => phr.HealthRiskId);

            builder.HasOne(x => x.SuspectedDisease)
               .WithMany(x => x.HealthRiskSuspectedDiseases)
               .HasForeignKey(phr => phr.SuspectedDiseaseId);
        }
    }
}
