namespace Zap.Tests.Unit.Projects;

public sealed class UpdateProjectRequestValidatorTests
{
    private readonly UpdateProject.RequestValidator _validator = new();

    [Fact]
    public void Validate_When_Request_Is_Valid_Passes()
    {
        var result = _validator.Validate(new UpdateProject.Request(
            "Project",
            "Description",
            "High",
            DateTime.UtcNow.AddMinutes(10)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_When_Name_Is_Empty_Fails()
    {
        var result = _validator.Validate(new UpdateProject.Request(
            string.Empty,
            "Description",
            "High",
            DateTime.UtcNow.AddMinutes(10)));

        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_When_Description_Is_Too_Long_Fails()
    {
        var result = _validator.Validate(new UpdateProject.Request(
            "Project",
            new string('a', 1001),
            "High",
            DateTime.UtcNow.AddMinutes(10)));

        Assert.Contains(result.Errors, error => error.PropertyName == "Description");
    }

    [Fact]
    public void Validate_When_DueDate_Is_In_The_Past_Fails()
    {
        var result = _validator.Validate(new UpdateProject.Request(
            "Project",
            "Description",
            "High",
            DateTime.UtcNow.AddMinutes(-10)));

        Assert.Contains(result.Errors, error => error.PropertyName == "DueDate");
    }
}
