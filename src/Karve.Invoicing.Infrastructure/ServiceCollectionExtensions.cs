using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Karve.Invoicing.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoicingInfrastructure(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddInvoicingDbContext(optionsAction);

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static IServiceCollection AddInvoicingDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<InvoicingDbContext>(optionsAction);
        return services;
    }
}