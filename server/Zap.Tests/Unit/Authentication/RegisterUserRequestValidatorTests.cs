namespace Zap.Tests.Unit.Authentication;

public sealed class RegisterUserRequestValidatorTests
{
    private readonly RegisterUser.RequestValidator _validator = new();

    [Fact]
    public void Validate_When_Request_Is_Valid_Passes()
    {
        var result = _validator.Validate(new RegisterUser.Request(
            "user@test.com",
            "secret1",
            "John",
            "Doe"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_When_Email_Is_Invalid_Fails()
    {
        var result = _validator.Validate(new RegisterUser.Request(
            "invalid-email",
            "secret1",
            "John",
            "Doe"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
    }

    [Fact]
    public void Validate_When_Password_Is_Too_Short_Fails()
    {
        var result = _validator.Validate(new RegisterUser.Request(
            "user@test.com",
            "123",
            "John",
            "Doe"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void Validate_When_FirstName_Is_Null_Fails()
    {
        var result = _validator.Validate(new RegisterUser.Request(
            "user@test.com",
            "secret1",
            null!,
            "Doe"));

        Assert.Contains(result.Errors, error => error.PropertyName == "FirstName");
    }

    [Fact]
    public void Validate_When_LastName_Is_Null_Fails()
    {
        var result = _validator.Validate(new RegisterUser.Request(
            "user@test.com",
            "secret1",
            "John",
            null!));

        Assert.Contains(result.Errors, error => error.PropertyName == "LastName");
    }
}
