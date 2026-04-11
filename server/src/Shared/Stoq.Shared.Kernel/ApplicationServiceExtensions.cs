using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Stoq.Shared.Kernel.Behaviours;

namespace Stoq.Shared.Kernel;
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
