using Microsoft.Extensions.DependencyInjection;

namespace Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositoryLayer(this IServiceCollection services)
    {
        return services;
    }
}
