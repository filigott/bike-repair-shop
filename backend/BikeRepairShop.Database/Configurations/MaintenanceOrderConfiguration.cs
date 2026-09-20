using BikeRepairShop.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeRepairShop.Database.Configurations;

public sealed class MaintenanceOrderConfiguration : IEntityTypeConfiguration<MaintenanceOrder>
{
    public const int EnumMaxLength = 32;

    public void Configure(EntityTypeBuilder<MaintenanceOrder> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerName)
            .HasMaxLength(MaintenanceOrder.CustomerNameMaxLength);

        builder.Property(o => o.PhoneNumber)
            .HasConversion(p => p.Value, v => PhoneNumber.Parse(v))
            .HasMaxLength(PhoneNumber.MaxLength);

        builder.Property(o => o.Email)
            .HasConversion(e => e.Value, v => EmailAddress.Parse(v))
            .HasMaxLength(EmailAddress.MaxLength);

        builder.Property(o => o.BikeBrand)
            .HasMaxLength(MaintenanceOrder.BikeBrandMaxLength);

        builder.Property(o => o.ServiceType)
            .HasConversion<string>()
            .HasMaxLength(EnumMaxLength);

        builder.Property(o => o.Notes)
            .HasMaxLength(MaintenanceOrder.NotesMaxLength);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumMaxLength);

        builder.HasOne<ServiceTypeDefinition>()
            .WithMany()
            .HasForeignKey(o => o.ServiceType)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
