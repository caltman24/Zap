﻿using FluentValidation;

namespace Zap.Api.Filters;

public class ValidationFilter<TRequest> : IEndpointFilter
{
    private readonly IValidator<TRequest> _validator;

    public ValidationFilter(IValidator<TRequest> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().First();
        var result = await _validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}

public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithRequestValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ValidationFilter<TRequest>>()
            .ProducesValidationProblem();
    }
}