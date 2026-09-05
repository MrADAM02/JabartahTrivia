using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Microsoft.Extensions.DependencyInjection;

namespace Jabartah.Trivia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Assembly-scanned instead of hand-registered -- we crossed the ~15-20 handler range
        // a while back (was 27 lines here, heading to 30+) where hand-adding every new
        // command/query handler stopped paying for itself.
        services.Scan(scan => scan
            .FromAssemblyOf<CreateGameSessionHandler>()
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>))).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))).AsImplementedInterfaces().WithScopedLifetime());

        return services;
    }
}
