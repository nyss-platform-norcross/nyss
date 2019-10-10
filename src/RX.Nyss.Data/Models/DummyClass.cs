using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models
{
    // This is just a dummy class to test that migrations will work.
    public class DummyClass
    {
        public int DummyColumn { get; set; }
    }

    public class DummyMap : IEntityTypeConfiguration<DummyClass>{
        public void Configure(EntityTypeBuilder<DummyClass> builder) => builder.HasKey(d => d.DummyColumn);
    }
}
