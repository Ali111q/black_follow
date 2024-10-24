using black_follow.Interface;
using black_follow.Repository;
using black_follow.Service;

namespace black_follow.Extensions;
public static class ServiceCollectionExtensions
{
    // Extension method for adding scoped services
    public static IServiceCollection AddCustomScopes(this IServiceCollection services)
    {
        // Example of adding services with different scopes

        // Singleton: one instance for the lifetime of the application
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
    

        return services;
    }
}