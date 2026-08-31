using Microsoft.Extensions.DependencyInjection;

namespace Jabartah.Trivia.Application.Abstractions;

public class Dispatcher(IServiceProvider provider) : IDispatcher
{
    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = provider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)command, ct);
    }

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = provider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)query, ct);
    }
}
