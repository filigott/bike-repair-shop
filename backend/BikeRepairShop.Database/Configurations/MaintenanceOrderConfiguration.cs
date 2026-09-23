using BikeRepairShop.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeRepairShop.Database.Configurations;

public sealed class MaintenanceOrderConfiguration : IEntityTypeConfiguration<MaintenanceOrder>
{
    public const int EnumMaxLength = 32;
    public const long OrderNumberStartValue = 1000;

    public void Configure(EntityTypeBuilder<MaintenanceOrder> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: OrderNumberStartValue);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.CustomerName)
            .HasMaxLength(MaintenanceOrder.CustomerNameMaxLength);

        builder.ComplexProperty(o => o.PhoneNumber, phone => phone
            .Property(p => p.Value)
            .HasColumnName("phone_number")
            .HasMaxLength(PhoneNumber.MaxLength));

        builder.ComplexProperty(o => o.Email, email => email
            .Property(e => e.Value)
            .HasColumnName("email")
            .HasMaxLength(EmailAddress.MaxLength));

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

        builder.Property(o => o.Version)
            .IsRowVersion()
            .HasColumnName("xmin");

        builder.HasOne<ServiceTypeDefinition>()
            .WithMany()
            .HasForeignKey(o => o.ServiceType)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
