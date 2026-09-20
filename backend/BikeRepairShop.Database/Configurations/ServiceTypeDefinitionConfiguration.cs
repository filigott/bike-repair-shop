using BikeRepairShop.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeRepairShop.Database.Configurations;

public sealed class ServiceTypeDefinitionConfiguration : IEntityTypeConfiguration<ServiceTypeDefinition>
{
    public void Configure(EntityTypeBuilder<ServiceTypeDefinition> builder)
    {
        builder.ToTable("service_types");

        builder.HasKey(s => s.Code);

        builder.Property(s => s.Code)
            .HasConversion<string>()
            .HasMaxLength(MaintenanceOrderConfiguration.EnumMaxLength);

        builder.Property(s => s.DisplayName)
            .HasMaxLength(ServiceTypeDefinition.DisplayNameMaxLength);

        // Anonymous rows: the entity has no public constructor, and HasData only needs the column values.
        builder.HasData(
            new { Code = ServiceType.WheelAdjustment, DisplayName = "Wheel adjustment", SortOrder = 1 },
            new { Code = ServiceType.ChainReplacement, DisplayName = "Chain replacement", SortOrder = 2 },
            new { Code = ServiceType.BrakeMaintenance, DisplayName = "Brake maintenance", SortOrder = 3 });
    }
}
