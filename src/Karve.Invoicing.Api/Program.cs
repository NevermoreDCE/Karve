using FluentValidation;
using Karve.Invoicing.Api.Middleware;
using Karve.Invoicing.Application;
using Karve.Invoicing.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.IO;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInvoicingDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddAutoMapper(typeof(AssemblyMarker));
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();
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

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Karve Invoicing API";
        options.DisableAgent();
        //options.IncludeXmlComments = true;
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowLocalhost3000");
app.MapHealthChecks("/health");



app.Run();
