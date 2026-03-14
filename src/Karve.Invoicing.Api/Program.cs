
using FluentValidation;
using Karve.Invoicing.Api.Middleware;
using Karve.Invoicing.Api.Services;
using Karve.Invoicing.Application;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;

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
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireCompanyMembership", policy =>
                policy.RequireAssertion(context => HasCompanyMembership(context)));
        });

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(new AuthorizeFilter("RequireCompanyMembership"));
        });
        builder.Services.AddAutoMapper(typeof(AssemblyMarker));
        builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                ConfigureOAuth2SecurityScheme(document, builder.Configuration);
                return Task.CompletedTask;
            });
        });
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
                ConfigureScalarOAuth(options, builder.Configuration);
                //options.IncludeXmlComments = true;
            });
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors("AllowLocalhost3000");
        app.UseAuthentication();
        app.UseMiddleware<UserProvisioningMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapControllers();

        if (!builder.Configuration.GetValue<bool>("DisableDataSeeding"))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            db.Database.EnsureCreated();
            DataSeeder.Seed(db);
        }

        app.Run();
    }

    private static bool HasCompanyMembership(AuthorizationHandlerContext context)
    {
        var httpContext = context.Resource switch
        {
            HttpContext directContext => directContext,
            AuthorizationFilterContext mvcContext => mvcContext.HttpContext,
            _ => null
        };

        if (httpContext is null)
        {
            return false;
        }

        var currentUserService = httpContext.RequestServices.GetService<ICurrentUserService>();
        return currentUserService?.CompanyIds.Any() == true;
    }

    private static void ConfigureOAuth2SecurityScheme(OpenApiDocument document, IConfiguration configuration)
    {
        if (!TryBuildAzureAdEndpoints(configuration, out var authorizationUrl, out var tokenUrl))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var scope = BuildOAuthScope(configuration);
        var oauthScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Azure AD OAuth2 authorization code flow with PKCE.",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = authorizationUrl,
                    TokenUrl = tokenUrl,
                    Scopes = string.IsNullOrWhiteSpace(scope)
                        ? new Dictionary<string, string>()
                        : new Dictionary<string, string>
                        {
                            [scope] = "Access Karve Invoicing API"
                        }
                }
            }
        };

        document.Components.SecuritySchemes["oauth2"] = oauthScheme;
        document.Security ??= new List<OpenApiSecurityRequirement>();

        var requiredScopes = string.IsNullOrWhiteSpace(scope)
            ? new List<string>()
            : new List<string> { scope };

        var oauthSchemeReference = new OpenApiSecuritySchemeReference("oauth2", document, null);
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [oauthSchemeReference] = requiredScopes
        });
    }

    private static void ConfigureScalarOAuth(ScalarOptions options, IConfiguration configuration)
    {
        if (!TryBuildAzureAdEndpoints(configuration, out var authorizationUrl, out var tokenUrl))
        {
            return;
        }

        var oauthClientId = configuration["OpenApi:OAuthClientId"];
        if (IsMissingOrPlaceholder(oauthClientId))
        {
            oauthClientId = configuration["AzureAd:ClientId"];
        }

        var scope = BuildOAuthScope(configuration);
        var scopes = string.IsNullOrWhiteSpace(scope)
            ? Array.Empty<string>()
            : new[] { scope };

        options.AddAuthorizationCodeFlow("oauth2", flow =>
        {
            flow.AuthorizationUrl = authorizationUrl.AbsoluteUri;
            flow.TokenUrl = tokenUrl.AbsoluteUri;
            flow.Pkce = Pkce.Sha256;

            if (!IsMissingOrPlaceholder(oauthClientId))
            {
                flow.ClientId = oauthClientId;
            }

            flow.SelectedScopes = scopes;
        });

        options.AddPreferredSecuritySchemes(new[] { "oauth2" });
        options.AddDefaultScopes("oauth2", scopes);
    }

    private static string BuildOAuthScope(IConfiguration configuration)
    {
        var configuredScope = configuration["OpenApi:OAuthScope"];
        if (!IsMissingOrPlaceholder(configuredScope))
        {
            return configuredScope!;
        }

        var audience = configuration["AzureAd:Audience"];
        if (!IsMissingOrPlaceholder(audience))
        {
            return $"api://{audience}/user_impersonation";
        }

        var clientId = configuration["AzureAd:ClientId"];
        if (!IsMissingOrPlaceholder(clientId))
        {
            return $"api://{clientId}/user_impersonation";
        }

        return string.Empty;
    }

    private static bool TryBuildAzureAdEndpoints(
        IConfiguration configuration,
        [NotNullWhen(true)] out Uri? authorizationUrl,
        [NotNullWhen(true)] out Uri? tokenUrl)
    {
        authorizationUrl = null;
        tokenUrl = null;

        var instance = configuration["AzureAd:Instance"]?.TrimEnd('/');
        var tenantId = configuration["AzureAd:TenantId"];

        if (IsMissingOrPlaceholder(instance) || IsMissingOrPlaceholder(tenantId))
        {
            return false;
        }

        var authorizationUrlString = $"{instance}/{tenantId}/oauth2/v2.0/authorize";
        var tokenUrlString = $"{instance}/{tenantId}/oauth2/v2.0/token";

        return Uri.TryCreate(authorizationUrlString, UriKind.Absolute, out authorizationUrl)
            && Uri.TryCreate(tokenUrlString, UriKind.Absolute, out tokenUrl);
    }

    private static bool IsMissingOrPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.Contains('<') || value.Contains('>');
    }
}
