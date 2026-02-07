using FluentValidation;
using Zap.Api.Common.Constants;

namespace Zap.Api.Common.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> ValidateTicketType<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(t => TicketTypes.ToList().Contains(t))
            .WithMessage($"Value must be one of: {string.Join(", ", TicketTypes.ToList())}");
    }

    public static IRuleBuilderOptions<T, string> ValidateTicketPriority<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(t => Priorities.ToList().Contains(t))
            .WithMessage($"Value must be one of: {string.Join(", ", Priorities.ToList())}");
    }

    public static IRuleBuilderOptions<T, string> ValidateTicketStatus<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(t => TicketStatuses.ToList().Contains(t))
            .WithMessage($"Value must be one of: {string.Join(", ", TicketStatuses.ToList())}");
    }
}