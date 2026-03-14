
using FluentValidation;
using Karve.Invoicing.Api.Middleware;
using Karve.Invoicing.Api.Services;
using Karve.Invoicing.Application;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;

/// <summary>
/// Entry point for the Karve Invoicing API application.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Main entry method. Configures and runs the web application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddInvoicingInfrastructure(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        builder.Services.AddControllers();
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

        //var db = app.Services.GetRequiredService<InvoicingDbContext>();
        //db.Database.EnsureCreated();
        
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            db.Database.EnsureCreated();
            DataSeeder.Seed(db);
        }

        app.Run();
    }
}
