using FluentAssertions;
using LemonTodo.Application.DTOs;
using LemonTodo.Application.Validators;

namespace LemonTodo.Application.Tests;

public class CreateTaskValidatorTests
{
    private readonly CreateTaskValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new CreateTaskRequest("Task", "Desc", new DateOnly(2026, 3, 1));
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Name_Fails()
    {
        var request = new CreateTaskRequest("", null, new DateOnly(2026, 3, 1));
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Name_Over200_Fails()
    {
        var request = new CreateTaskRequest(new string('x', 201), null, new DateOnly(2026, 3, 1));
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Description_Over2000_Fails()
    {
        var request = new CreateTaskRequest("Task", new string('x', 2001), new DateOnly(2026, 3, 1));
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Default_CompletionDate_Fails()
    {
        var request = new CreateTaskRequest("Task", null, default);
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}

public class UpdateTaskValidatorTests
{
    private readonly UpdateTaskValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new UpdateTaskRequest("Task", "Desc", new DateOnly(2026, 3, 1));
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Name_Fails()
    {
        var request = new UpdateTaskRequest("", null, new DateOnly(2026, 3, 1));
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
