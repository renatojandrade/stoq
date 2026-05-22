using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Stoq.Shopping.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddShoppingApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
