using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Stoq.Identity.Application.Ports;
using Stoq.Identity.Infrastructure.Entities;
using Stoq.Identity.Infrastructure.Persistence;
using Stoq.Identity.Infrastructure.Persistence.Interceptors;
using Stoq.Identity.Infrastructure.Services;

namespace Stoq.Identity.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ResendClientOptions>()
            .Configure<IConfiguration>((opt, cfg) => opt.ApiToken = cfg["Resend:ApiKey"] ?? string.Empty);
        services.AddHttpClient<ResendClient>();
        services.AddTransient<IResend, ResendClient>();

        services.AddAspNetIdentity();
        services.AddIdentityAuthentication(configuration);
        services.AddIdentityMassTransit(configuration);
        services.AddPortImplementations();
        services.AddHttpClient<ISocialAuthService, SocialAuthServiceImpl>();

        return services;
    }

    public static IServiceCollection AddIdentityDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<IdentityModuleDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        return services;
    }

    private static IServiceCollection AddAspNetIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityModuleDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddIdentityAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var signingKey = configuration["Identity:JwtSigningKey"]
            ?? throw new InvalidOperationException("Identity:JwtSigningKey is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Identity:JwtIssuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Identity:JwtAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddIdentityMassTransit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<IdentityModuleDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var host = configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrEmpty(host))
                {
                    cfg.Host(new Uri(host));
                }

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    private static IServiceCollection AddPortImplementations(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityServiceImpl>();
        services.AddScoped<IIdentityRepository, IdentityRepositoryImpl>();
        services.AddScoped<ITokenService, TokenServiceImpl>();
        services.AddScoped<IEmailService, EmailServiceImpl>();

        return services;
    }
}
