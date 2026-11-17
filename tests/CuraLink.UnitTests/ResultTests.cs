using CuraLink.Application.Common;

namespace CuraLink.UnitTests;

public class ResultTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResultWithoutErrors()
    {
        var result = Result.Ok();
        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void FailureWithParams_CreatesFailedResultWithErrors()
    {
        var result = Result.Failure("first", "second");
        Assert.False(result.Succeeded);
        Assert.Equal(new[] { "first", "second" }, result.Errors);
    }

    [Fact]
    public void FailureWithEnumerable_CreatesFailedResultWithErrors()
    {
        var result = Result.Failure(new List<string> { "first", "second" });
        Assert.False(result.Succeeded);
        Assert.Equal(new[] { "first", "second" }, result.Errors);
    }

    [Fact]
    public void GenericOk_CreatesSuccessfulResultWithValue()
    {
        var result = Result<int>.Ok(42);
        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GenericFailureWithParams_CreatesFailedResultWithoutValue()
    {
        var result = Result<string>.Failure("missing");
        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(new[] { "missing" }, result.Errors);
    }

    [Fact]
    public void GenericFailureWithEnumerable_CreatesFailedResultWithErrors()
    {
        var result = Result<string>.Failure(new List<string> { "missing", "invalid" });
        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Equal(new[] { "missing", "invalid" }, result.Errors);
    }

    [Fact]
    public void GenericFailure_CanBeUsedAsBaseResult()
    {
        Result result = Result<string>.Failure("missing");
        Assert.False(result.Succeeded);
        Assert.Equal(new[] { "missing" }, result.Errors);
    }
}
