namespace Service.DTOs.Common;

public class ServiceResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = [];

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };

    public static ServiceResult<T> Fail(IEnumerable<string> errors) => new()
    {
        Success = false,
        Errors = errors.ToList()
    };

    public static ServiceResult<T> Fail(params string[] errors) => Fail((IEnumerable<string>)errors);
}
