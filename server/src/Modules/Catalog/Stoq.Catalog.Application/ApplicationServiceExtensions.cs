using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Stoq.Catalog.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
