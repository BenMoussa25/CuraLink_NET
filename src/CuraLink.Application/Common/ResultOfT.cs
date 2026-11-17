namespace CuraLink.Application.Common;

public sealed class Result<T> : Result
{
    private Result(T? value, bool succeeded, IReadOnlyList<string>? errors) : base(succeeded, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Ok(T value) => new(value, true, null);

    public new static Result<T> Failure(params string[] errors) => new(default, false, errors);

    public new static Result<T> Failure(IEnumerable<string> errors) => new(default, false, errors.ToList());
}
