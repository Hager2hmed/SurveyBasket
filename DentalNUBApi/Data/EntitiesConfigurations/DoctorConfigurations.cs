using DentalNUB.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalNUB.Api.Data.EntitiesConfigurations;

public class DoctorConfigurations : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasIndex(e => e.DoctorEmail).IsUnique();
        builder.HasIndex(p => p.DoctorPhone).IsUnique();

    }
}
