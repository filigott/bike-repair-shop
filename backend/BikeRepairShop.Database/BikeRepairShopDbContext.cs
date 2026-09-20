using BikeRepairShop.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Database;

public sealed class BikeRepairShopDbContext(DbContextOptions<BikeRepairShopDbContext> options) : DbContext(options)
{
    public DbSet<MaintenanceOrder> Orders => Set<MaintenanceOrder>();

    public DbSet<ServiceTypeDefinition> ServiceTypes => Set<ServiceTypeDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BikeRepairShopDbContext).Assembly);
    }
}
