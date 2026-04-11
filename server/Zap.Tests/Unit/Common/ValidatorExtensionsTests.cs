namespace Zap.Tests.Unit.Common;

public sealed class ValidatorExtensionsTests
{
    [Theory]
    [InlineData(TicketTypes.Defect)]
    [InlineData(TicketTypes.Feature)]
    public void ValidateTicketType_When_Value_Is_Valid_Passes(string value)
    {
        var validator = new TicketTypeValidator();

        var result = validator.Validate(new TestModel(value));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateTicketType_When_Value_Is_Invalid_Fails()
    {
        var validator = new TicketTypeValidator();

        var result = validator.Validate(new TestModel("Epic"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Value must be one of"));
    }

    [Theory]
    [InlineData(Priorities.Low)]
    [InlineData(Priorities.Urgent)]
    public void ValidateTicketPriority_When_Value_Is_Valid_Passes(string value)
    {
        var validator = new TicketPriorityValidator();

        var result = validator.Validate(new TestModel(value));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateTicketPriority_When_Value_Is_Invalid_Fails()
    {
        var validator = new TicketPriorityValidator();

        var result = validator.Validate(new TestModel("Critical"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Value must be one of"));
    }

    [Theory]
    [InlineData(TicketStatuses.New)]
    [InlineData(TicketStatuses.Resolved)]
    public void ValidateTicketStatus_When_Value_Is_Valid_Passes(string value)
    {
        var validator = new TicketStatusValidator();

        var result = validator.Validate(new TestModel(value));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateTicketStatus_When_Value_Is_Invalid_Fails()
    {
        var validator = new TicketStatusValidator();

        var result = validator.Validate(new TestModel("Waiting"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Value must be one of"));
    }

    private sealed record TestModel(string Value);

    private sealed class TicketTypeValidator : AbstractValidator<TestModel>
    {
        public TicketTypeValidator()
        {
            RuleFor(x => x.Value).ValidateTicketType();
        }
    }

    private sealed class TicketPriorityValidator : AbstractValidator<TestModel>
    {
        public TicketPriorityValidator()
        {
            RuleFor(x => x.Value).ValidateTicketPriority();
        }
    }

    private sealed class TicketStatusValidator : AbstractValidator<TestModel>
    {
        public TicketStatusValidator()
        {
            RuleFor(x => x.Value).ValidateTicketStatus();
        }
    }
}
