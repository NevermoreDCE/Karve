using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Karve.Invoicing.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicingDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<InvoicingDbContext>(optionsAction);
        return services;
    }
}