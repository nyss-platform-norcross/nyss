using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ProjectErrorMessageMap : IEntityTypeConfiguration<ProjectErrorMessage>
    {
        public void Configure(EntityTypeBuilder<ProjectErrorMessage> builder)
        {
            builder.HasKey("ProjectId", "MessageKey");

            builder.Property(x => x.MessageKey)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Message)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .IsRequired();
        }
    }
}
