using Karve.Invoicing.Application;
using Karve.Invoicing.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInvoicingDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddAutoMapper(typeof(AssemblyMarker));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Karve Invoicing API";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost3000");
app.MapHealthChecks("/health");



app.Run();
