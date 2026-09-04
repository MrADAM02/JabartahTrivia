using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Auth.DeleteAccount;
using Jabartah.Trivia.Application.Auth.GetAccount;
using Jabartah.Trivia.Application.Auth.Login;
using Jabartah.Trivia.Application.Auth.Register;
using Jabartah.Trivia.Application.Categories.CreateCustomCategory;
using Jabartah.Trivia.Application.Categories.DeleteCustomCategory;
using Jabartah.Trivia.Application.Categories.GetMyCategory;
using Jabartah.Trivia.Application.Categories.ListCategories;
using Jabartah.Trivia.Application.Categories.ListMyCategories;
using Jabartah.Trivia.Application.Categories.ListPasswordCategories;
using Jabartah.Trivia.Application.Categories.ListRankingCategories;
using Jabartah.Trivia.Application.Categories.ListTop100Categories;
using Jabartah.Trivia.Application.GameSessions.ActivateTimerDebuff;
using Jabartah.Trivia.Application.GameSessions.AwardPoints;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Jabartah.Trivia.Application.GameSessions.GetBoard;
using Jabartah.Trivia.Application.GameSessions.RevealAnswer;
using Jabartah.Trivia.Application.GameSessions.SelectQuestion;
using Jabartah.Trivia.Application.PasswordGame.ConsumeRevealToken;
using Jabartah.Trivia.Application.PasswordGame.CreatePasswordGameSession;
using Jabartah.Trivia.Application.PasswordGame.GetSession;
using Jabartah.Trivia.Application.PasswordGame.IssueRevealToken;
using Jabartah.Trivia.Application.PasswordGame.ResolveRound;
using Jabartah.Trivia.Application.PasswordGame.StartNextRound;
using Jabartah.Trivia.Application.PasswordGame.UseExtraTime;
using Jabartah.Trivia.Application.RankingGame.CreateRankingGameSession;
using Jabartah.Trivia.Application.RankingGame.GetSession;
using Jabartah.Trivia.Application.RankingGame.RevealPosition;
using Jabartah.Trivia.Application.RankingGame.StartNextRound;
using Jabartah.Trivia.Application.RankingGame.SubmitRound;
using Jabartah.Trivia.Application.Sessions.GetMySessions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Application.Top100Game.GetSession;
using Jabartah.Trivia.Application.Top100Game.StartNextRound;
using Jabartah.Trivia.Application.Top100Game.SubmitGuess;
using Microsoft.Extensions.DependencyInjection;

namespace Jabartah.Trivia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Explicit registration -- well past the ~15-20 range where this starts to get
        // unwieldy (27 handlers now). Switch to Scrutor assembly scanning on the next
        // feature that adds a handful more, rather than continuing to hand-add each one.
        services.AddScoped<ICommandHandler<RegisterCommand, AuthResult>, RegisterHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginHandler>();
        services.AddScoped<ICommandHandler<DeleteAccountCommand, bool>, DeleteAccountHandler>();
        services.AddScoped<IQueryHandler<GetAccountQuery, AccountDto>, GetAccountHandler>();

        services.AddScoped<ICommandHandler<CreateGameSessionCommand, CreateGameSessionResult>, CreateGameSessionHandler>();
        services.AddScoped<IQueryHandler<GetBoardQuery, BoardDto>, GetBoardHandler>();
        services.AddScoped<ICommandHandler<SelectQuestionCommand, SelectQuestionResult>, SelectQuestionHandler>();
        services.AddScoped<ICommandHandler<AwardPointsCommand, AwardPointsResult>, AwardPointsHandler>();
        services.AddScoped<ICommandHandler<ActivateTimerDebuffCommand, ActivateTimerDebuffResult>, ActivateTimerDebuffHandler>();
        services.AddScoped<ICommandHandler<RevealAnswerCommand, RevealAnswerResult>, RevealAnswerHandler>();
        services.AddScoped<IQueryHandler<ListCategoriesQuery, List<CategoryDto>>, ListCategoriesHandler>();

        services.AddScoped<ICommandHandler<CreatePasswordGameSessionCommand, CreatePasswordGameSessionResult>, CreatePasswordGameSessionHandler>();
        services.AddScoped<ICommandHandler<StartNextPasswordRoundCommand, StartNextPasswordRoundResult>, StartNextPasswordRoundHandler>();
        services.AddScoped<ICommandHandler<IssueRevealTokenCommand, IssueRevealTokenResult>, IssueRevealTokenHandler>();
        services.AddScoped<ICommandHandler<ResolvePasswordRoundCommand, ResolvePasswordRoundResult>, ResolvePasswordRoundHandler>();
        services.AddScoped<IQueryHandler<GetPasswordSessionQuery, PasswordSessionDto>, GetPasswordSessionHandler>();
        services.AddScoped<ICommandHandler<ConsumeRevealTokenCommand, ConsumeRevealTokenResult>, ConsumeRevealTokenHandler>();
        services.AddScoped<IQueryHandler<ListPasswordCategoriesQuery, List<PasswordCategoryDto>>, ListPasswordCategoriesHandler>();
        services.AddScoped<ICommandHandler<UseExtraTimeCommand, UseExtraTimeResult>, UseExtraTimeHandler>();

        services.AddScoped<ICommandHandler<CreateRankingGameSessionCommand, CreateRankingGameSessionResult>, CreateRankingGameSessionHandler>();
        services.AddScoped<ICommandHandler<StartNextRankingRoundCommand, StartNextRankingRoundResult>, StartNextRankingRoundHandler>();
        services.AddScoped<ICommandHandler<SubmitRankingRoundCommand, SubmitRankingRoundResult>, SubmitRankingRoundHandler>();
        services.AddScoped<IQueryHandler<GetRankingSessionQuery, RankingSessionDto>, GetRankingSessionHandler>();
        services.AddScoped<IQueryHandler<ListRankingCategoriesQuery, List<RankingCategoryDto>>, ListRankingCategoriesHandler>();
        services.AddScoped<ICommandHandler<RevealRankingPositionCommand, RevealRankingPositionResult>, RevealRankingPositionHandler>();

        services.AddScoped<ICommandHandler<CreateTop100GameSessionCommand, CreateTop100GameSessionResult>, CreateTop100GameSessionHandler>();
        services.AddScoped<ICommandHandler<StartNextTop100RoundCommand, StartNextTop100RoundResult>, StartNextTop100RoundHandler>();
        services.AddScoped<ICommandHandler<SubmitGuessCommand, SubmitGuessResult>, SubmitGuessHandler>();
        services.AddScoped<IQueryHandler<GetTop100SessionQuery, Top100SessionDto>, GetTop100SessionHandler>();
        services.AddScoped<IQueryHandler<ListTop100CategoriesQuery, List<Top100CategoryDto>>, ListTop100CategoriesHandler>();

        services.AddScoped<IQueryHandler<GetMySessionsQuery, List<MySessionDto>>, GetMySessionsHandler>();

        services.AddScoped<IQueryHandler<ListMyCategoriesQuery, List<CategoryDto>>, ListMyCategoriesHandler>();
        services.AddScoped<ICommandHandler<CreateCustomCategoryCommand, CreateCustomCategoryResult>, CreateCustomCategoryHandler>();
        services.AddScoped<IQueryHandler<GetMyCategoryQuery, MyCategoryDetailDto>, GetMyCategoryHandler>();
        services.AddScoped<ICommandHandler<DeleteCustomCategoryCommand, bool>, DeleteCustomCategoryHandler>();

        return services;
    }
}
