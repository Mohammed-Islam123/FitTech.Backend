namespace Shared.Wrappers ; 

public class Response<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static Response<T> SuccessResponse(T data, string? message = null)
    {
        return new Response<T> { Success = true, Data = data, Message = message };
    }

    public static  Response<T> FailResponse(string message, List<string>? errors = null, T? data = default)
    {
        return new Response<T> { Success = false, Data = data, Message = message, Errors = errors };
    }
}
