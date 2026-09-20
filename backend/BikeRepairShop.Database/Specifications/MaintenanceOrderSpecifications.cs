using System.Linq.Expressions;
using BikeRepairShop.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BikeRepairShop.Database.Specifications;

public static class MaintenanceOrderSpecifications
{
    public static Expression<Func<MaintenanceOrder, bool>> CustomerMatches(string term)
    {
        var pattern = $"%{EscapeLike(term)}%";
        var phonePattern = $"%{EscapeLike(term.Replace(" ", ""))}%";
        return o => EF.Functions.ILike(o.CustomerName, pattern)
            || EF.Functions.ILike(o.PhoneNumber.Value, phonePattern)
            || EF.Functions.ILike(o.Email.Value, pattern);
    }

    private static string EscapeLike(string value)
    {
        return value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
    }
}
