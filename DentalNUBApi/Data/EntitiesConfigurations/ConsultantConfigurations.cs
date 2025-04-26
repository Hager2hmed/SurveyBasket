using DentalNUB.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalNUB.Api.Data.EntitiesConfigurations;

public class ConsultantConfigurations : IEntityTypeConfiguration<Consultant>
{
    public void Configure(EntityTypeBuilder<Consultant> builder)
    {
        builder.HasIndex(e => e.ConsEmail).IsUnique();
        
    }
}
