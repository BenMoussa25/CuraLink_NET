namespace CuraLink.Application.Common;

public class Result
{
    protected Result(bool succeeded, IReadOnlyList<string>? errors)
    {
        Succeeded = succeeded;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static Result Ok() => new(true, null);

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToList());
}
