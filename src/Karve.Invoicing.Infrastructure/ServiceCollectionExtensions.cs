using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Karve.Invoicing.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicingDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InvoicingDbContext>(options =>
            options.UseSqlite(connectionString));
        return services;
    }
}