using Karve.Invoicing.Application.BackgroundJobs.Handlers;
using Karve.Invoicing.Application.BackgroundJobs.Jobs;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.Enums;
using Karve.Invoicing.Domain.ValueObjects;
using Karve.Invoicing.Infrastructure;
using Karve.Invoicing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karve.Invoicing.Application.Tests.BackgroundJobs;

public class BackgroundJobHandlerTests
{
    [Fact]
    public async Task SendInvoiceEmailJobHandler_ValidInvoice_CompletesSuccessfully()
    {
        var companyId = Guid.NewGuid();
        var context = CreateContext(companyId);
        var invoice = await SeedInvoiceAsync(context, companyId, DateTime.UtcNow.AddDays(10), InvoiceStatus.Sent);

        var repository = new InvoiceRepository(context, new TestCurrentUserService(companyId));
        var handler = new SendInvoiceEmailJobHandler(repository, NullLogger<SendInvoiceEmailJobHandler>.Instance);

        await handler.HandleAsync(new SendInvoiceEmailJob(invoice.Id, companyId), CancellationToken.None);

        var persistedInvoice = await context.Invoices.IgnoreQueryFilters().FirstAsync(i => i.Id == invoice.Id);
        Assert.Equal(invoice.Id, persistedInvoice.Id);
    }

    [Fact]
    public async Task CheckOverdueInvoicesJobHandler_OverdueInvoice_UpdatesStatusToOverdue()
    {
        var companyId = Guid.NewGuid();
        var context = CreateContext(companyId);
        var overdueInvoice = await SeedInvoiceAsync(context, companyId, DateTime.UtcNow.AddDays(-3), InvoiceStatus.Sent);

        var repository = new InvoiceRepository(context, new TestCurrentUserService(companyId));
        var handler = new CheckOverdueInvoicesJobHandler(repository, NullLogger<CheckOverdueInvoicesJobHandler>.Instance);

        await handler.HandleAsync(new CheckOverdueInvoicesJob(companyId), CancellationToken.None);

        var updated = await context.Invoices.IgnoreQueryFilters().FirstAsync(i => i.Id == overdueInvoice.Id);
        Assert.Equal(InvoiceStatus.Overdue, updated.Status);
    }

    private static InvoicingDbContext CreateContext(Guid companyId)
    {
        var options = new DbContextOptionsBuilder<InvoicingDbContext>()
            .UseInMemoryDatabase($"handlers-{companyId:N}")
            .Options;

        return new InvoicingDbContext(options, new TestCurrentUserService(companyId));
    }

    private static async Task<Invoice> SeedInvoiceAsync(
        InvoicingDbContext context,
        Guid companyId,
        DateTime dueDate,
        InvoiceStatus status)
    {
        var company = new Company
        {
            Id = companyId,
            Name = "Handler Test Company"
        };

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Contoso Customer",
            BillingAddress = "1 Main St",
            Email = new EmailAddress("customer@example.com")
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CustomerId = customer.Id,
            Customer = customer,
            Company = company,
            InvoiceNumber = 1001,
            InvoiceDate = DateTime.UtcNow.AddDays(-10),
            DueDate = dueDate,
            Status = status
        };

        context.Companies.Add(company);
        context.Customers.Add(customer);
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        return invoice;
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public TestCurrentUserService(Guid companyId)
        {
            CompanyIds = new List<Guid> { companyId };
        }

        public string? UserId => "test-user";

        public string? Email => "test@example.com";

        public IReadOnlyList<Guid> CompanyIds { get; }
    }
}
