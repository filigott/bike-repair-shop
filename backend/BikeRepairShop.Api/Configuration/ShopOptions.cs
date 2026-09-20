using System.ComponentModel.DataAnnotations;

namespace BikeRepairShop.Api.Configuration;

public sealed class ShopOptions
{
    public const string SectionName = "Shop";

    [Required]
    public string TimeZone { get; set; } = "";
}
