
using FluentValidation;
using Karve.Invoicing.Api.Middleware;
using Karve.Invoicing.Api.Observability;
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
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
        var serilogOptions = builder.Configuration
            .GetSection(SerilogOptions.SectionName)
            .Get<SerilogOptions>() ?? new SerilogOptions();
        var openTelemetryOptions = builder.Configuration
            .GetSection(OpenTelemetryOptions.SectionName)
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        // Add services to the container.

        builder.Services.Configure<SerilogOptions>(
            builder.Configuration.GetSection(SerilogOptions.SectionName));
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, _, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Is(ParseSerilogLevel(serilogOptions.MinimumLevel, LogEventLevel.Information))
                .MinimumLevel.Override("Microsoft", ParseSerilogLevel(serilogOptions.MicrosoftMinimumLevel, LogEventLevel.Warning))
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", openTelemetryOptions.ServiceName)
                .Enrich.WithProperty("Environment", string.IsNullOrWhiteSpace(openTelemetryOptions.Environment)
                    ? builder.Environment.EnvironmentName
                    : openTelemetryOptions.Environment);

            if (serilogOptions.EnableJsonConsole)
            {
                loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
            }
            else
            {
                loggerConfiguration.WriteTo.Console();
            }
        }, writeToProviders: true);

        builder.Services.Configure<OpenTelemetryOptions>(
            builder.Configuration.GetSection(OpenTelemetryOptions.SectionName));
        builder.AddKarveOpenTelemetry();
        builder.Logging.Configure(options =>
        {
            options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId
                | ActivityTrackingOptions.SpanId
                | ActivityTrackingOptions.ParentId
                | ActivityTrackingOptions.Baggage
                | ActivityTrackingOptions.Tags;
        });

        builder.Services.AddInvoicingInfrastructure(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var existingEvents = options.Events;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        if (existingEvents?.OnAuthenticationFailed is not null)
                        {
                            await existingEvents.OnAuthenticationFailed(context);
                        }

                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerDiagnostics");

                        logger.LogError(
                            context.Exception,
                            "JWT authentication failed for {Path}.",
                            context.HttpContext.Request.Path);
                    },
                    OnChallenge = async context =>
                    {
                        if (existingEvents?.OnChallenge is not null)
                        {
                            await existingEvents.OnChallenge(context);
                        }

                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearerDiagnostics");

                        logger.LogWarning(
                            "JWT challenge triggered for {Path}. Error: {Error}. Description: {Description}.",
                            context.HttpContext.Request.Path,
                            context.Error,
                            context.ErrorDescription);
                    }
                };
            });
        }
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
        builder.Services.AddAutoMapper(_ => { }, typeof(AssemblyMarker).GetTypeInfo().Assembly);
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
                policy.WithOrigins("https://localhost:3000")
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

            app.Use(async (context, next) =>
            {
                var isScalarCallback = context.Request.Path.StartsWithSegments("/scalar/v1")
                    && (context.Request.Query.ContainsKey("code")
                        || context.Request.Query.ContainsKey("error")
                        || context.Request.Query.ContainsKey("state"));

                if (isScalarCallback)
                {
                    var logger = context.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("ScalarOAuthDiagnostics");

                    var state = context.Request.Query["state"].ToString();
                    var statePreview = state.Length > 12 ? state[..12] : state;
                    var error = context.Request.Query["error"].ToString();
                    var errorDescription = context.Request.Query["error_description"].ToString();

                    logger.LogInformation(
                        "Scalar OAuth callback reached. Path={Path}, HasCode={HasCode}, StatePreview={StatePreview}, Error={Error}, ErrorDescription={ErrorDescription}",
                        context.Request.Path,
                        context.Request.Query.ContainsKey("code"),
                        statePreview,
                        string.IsNullOrWhiteSpace(error) ? "<none>" : error,
                        string.IsNullOrWhiteSpace(errorDescription) ? "<none>" : errorDescription);
                }

                await next();
            });
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<CorrelationIdMiddleware>();
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

    private static LogEventLevel ParseSerilogLevel(string? value, LogEventLevel fallback)
    {
        return Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var level)
            ? level
            : fallback;
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
        var redirectUri = BuildOAuthRedirectUri(configuration);

        options.AddAuthorizationCodeFlow("oauth2", flow =>
        {
            flow.AuthorizationUrl = authorizationUrl.AbsoluteUri;
            flow.TokenUrl = tokenUrl.AbsoluteUri;
            flow.Pkce = Pkce.Sha256;
            flow.WithCredentialsLocation(CredentialsLocation.Body);

            if (!IsMissingOrPlaceholder(oauthClientId))
            {
                var safeOAuthClientId = oauthClientId!;
                flow.ClientId = safeOAuthClientId;
                // Scalar 2.13 can omit client_id in token exchange for some providers.
                // Add an explicit body parameter so Azure AD always receives it.
                flow.AddBodyParameter("client_id", safeOAuthClientId);
            }

            if (!string.IsNullOrWhiteSpace(redirectUri))
            {
                flow.RedirectUri = redirectUri;
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

    private static string BuildOAuthRedirectUri(IConfiguration configuration)
    {
        var configuredRedirectUri = configuration["OpenApi:OAuthRedirectUri"];
        if (!IsMissingOrPlaceholder(configuredRedirectUri))
        {
            return configuredRedirectUri!;
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
