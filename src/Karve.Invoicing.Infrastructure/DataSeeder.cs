using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.Enums;
using Karve.Invoicing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure;

public static class DataSeeder
{
    public static void Seed(InvoicingDbContext db)
    {
        if (db.Companies.IgnoreQueryFilters().Any())
            return;

        // ── Companies ───────────────────────────────────────────────────────────
        var companies = new List<Company>
        {
            new() { Id = Guid.NewGuid(), Name = "Acme Corporation" },
            new() { Id = Guid.NewGuid(), Name = "Globex Industries" },
            new() { Id = Guid.NewGuid(), Name = "Initech Solutions" },
        };
        db.Companies.AddRange(companies);
        db.SaveChanges();

        var acme    = companies[0];
        var globex  = companies[1];
        var initech = companies[2];

        // ── Products (6) ────────────────────────────────────────────────────────
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), CompanyId = acme.Id,    Name = "Widget Pro",       Sku = "WGT-001", UnitPrice = new Money(49.99m,  "USD") },
            new() { Id = Guid.NewGuid(), CompanyId = acme.Id,    Name = "Gadget Basic",     Sku = "GDG-002", UnitPrice = new Money(19.99m,  "USD") },
            new() { Id = Guid.NewGuid(), CompanyId = globex.Id,  Name = "Doohickey Deluxe", Sku = "DHK-003", UnitPrice = new Money(99.00m,  "USD") },
            new() { Id = Guid.NewGuid(), CompanyId = globex.Id,  Name = "Thingamajig",      Sku = "TMJ-004", UnitPrice = new Money(34.50m,  "USD") },
            new() { Id = Guid.NewGuid(), CompanyId = initech.Id, Name = "TPS Report Cover", Sku = "TPS-005", UnitPrice = new Money(4.99m,   "USD") },
            new() { Id = Guid.NewGuid(), CompanyId = initech.Id, Name = "Stapler (Red)",    Sku = "STR-006", UnitPrice = new Money(12.50m,  "USD") },
        };
        db.Products.AddRange(products);
        db.SaveChanges();

        // ── Customers (5) ───────────────────────────────────────────────────────
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(), CompanyId = acme.Id,
                Name = "John Smith",
                Email = new EmailAddress("john.smith@example.com"),
                BillingAddress = "100 Main St, Springfield, IL 62701"
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyId = acme.Id,
                Name = "Sara Connor",
                Email = new EmailAddress("sara.connor@example.com"),
                BillingAddress = "200 Oak Ave, Chicago, IL 60601"
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyId = globex.Id,
                Name = "Bob Johnson",
                Email = new EmailAddress("bob.johnson@example.com"),
                BillingAddress = "300 Elm Rd, Shelbyville, IL 62565"
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyId = globex.Id,
                Name = "Alice Williams",
                Email = new EmailAddress("alice.williams@example.com"),
                BillingAddress = "400 Pine Blvd, Peoria, IL 61602"
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyId = initech.Id,
                Name = "Michael Bolton",
                Email = new EmailAddress("michael.bolton@initech.example.com"),
                BillingAddress = "501 Software Dr, Austin, TX 73301"
            },
        };
        db.Customers.AddRange(customers);
        db.SaveChanges();

        // ── Invoices (12) with 3–4 line items each ──────────────────────────────
        // Helper: pick products that belong to the same company as the invoice
        static List<Product> ProductsFor(Guid companyId, List<Product> all) =>
            all.Where(p => p.CompanyId == companyId).ToList();

        var invoiceSeeds = new[]
        {
            // Acme – John Smith (4 invoices)
            (acme.Id, customers[0].Id, DateTime.UtcNow.AddDays(-60),  DateTime.UtcNow.AddDays(-30),  InvoiceStatus.Paid),
            (acme.Id, customers[0].Id, DateTime.UtcNow.AddDays(-30),  DateTime.UtcNow.AddDays(0),    InvoiceStatus.Overdue),
            (acme.Id, customers[1].Id, DateTime.UtcNow.AddDays(-14),  DateTime.UtcNow.AddDays(16),   InvoiceStatus.Sent),
            (acme.Id, customers[1].Id, DateTime.UtcNow.AddDays(-2),   DateTime.UtcNow.AddDays(28),   InvoiceStatus.Draft),
            // Globex – Bob Johnson / Alice Williams (4 invoices)
            (globex.Id, customers[2].Id, DateTime.UtcNow.AddDays(-90),  DateTime.UtcNow.AddDays(-60), InvoiceStatus.Paid),
            (globex.Id, customers[2].Id, DateTime.UtcNow.AddDays(-45),  DateTime.UtcNow.AddDays(-15), InvoiceStatus.Overdue),
            (globex.Id, customers[3].Id, DateTime.UtcNow.AddDays(-20),  DateTime.UtcNow.AddDays(10),  InvoiceStatus.Viewed),
            (globex.Id, customers[3].Id, DateTime.UtcNow.AddDays(-5),   DateTime.UtcNow.AddDays(25),  InvoiceStatus.Sent),
            // Initech – Michael Bolton (4 invoices)
            (initech.Id, customers[4].Id, DateTime.UtcNow.AddDays(-100), DateTime.UtcNow.AddDays(-70), InvoiceStatus.Paid),
            (initech.Id, customers[4].Id, DateTime.UtcNow.AddDays(-50),  DateTime.UtcNow.AddDays(-20), InvoiceStatus.Paid),
            (initech.Id, customers[4].Id, DateTime.UtcNow.AddDays(-10),  DateTime.UtcNow.AddDays(20),  InvoiceStatus.Sent),
            (initech.Id, customers[4].Id, DateTime.UtcNow.AddDays(-1),   DateTime.UtcNow.AddDays(29),  InvoiceStatus.Draft),
        };

        var lineItems   = new List<InvoiceLineItem>();
        var payments    = new List<Payment>();
        var invoiceList = new List<Invoice>();

        var rng = new Random(42);

        foreach (var (companyId, customerId, invoiceDate, dueDate, status) in invoiceSeeds)
        {
            var invoice = new Invoice
            {
                Id          = Guid.NewGuid(),
                CompanyId   = companyId,
                CustomerId  = customerId,
                InvoiceDate = invoiceDate,
                DueDate     = dueDate,
                Status      = status,
            };
            invoiceList.Add(invoice);
            db.Invoices.Add(invoice);
            db.SaveChanges(); // save to get FK available for line items

            var companyProducts = ProductsFor(companyId, products);
            // 3 or 4 line items per invoice
            int lineCount = rng.Next(3, 5);
            decimal invoiceTotal = 0m;

            for (int i = 0; i < lineCount; i++)
            {
                var product = companyProducts[i % companyProducts.Count];
                int qty = rng.Next(1, 6);
                lineItems.Add(new InvoiceLineItem
                {
                    Id        = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ProductId = product.Id,
                    Quantity  = qty,
                    UnitPrice = product.UnitPrice,
                });
                invoiceTotal += product.UnitPrice.Amount * qty;
            }

            // Add a payment for Paid invoices
            if (status == InvoiceStatus.Paid)
            {
                payments.Add(new Payment
                {
                    Id          = Guid.NewGuid(),
                    CompanyId   = companyId,
                    InvoiceId   = invoice.Id,
                    Amount      = new Money(invoiceTotal, "USD"),
                    PaymentDate = dueDate.AddDays(-rng.Next(1, 5)),
                    Method      = (PaymentMethod)rng.Next(0, 4),
                });
            }
        }

        db.InvoiceLineItems.AddRange(lineItems);
        db.Payments.AddRange(payments);
        db.SaveChanges();
    }
}
