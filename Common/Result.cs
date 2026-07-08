namespace Authentication.Mfa.Twilio.Common;

public class Result
{
    protected Result(bool isSuccess, string message, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public string? ErrorCode { get; }

    public static Result Success(string message = "Operation completed successfully.") => new(true, message);
    public static Result Failure(string message, string? errorCode = null) => new(false, message, errorCode);

    public static Result<T> Success<T>(T value, string message = "Operation completed successfully.") => new(value, true, message);
    public static Result<T> Failure<T>(string message, string? errorCode = null) => new(default!, false, message, errorCode);
}

public class Result<T> : Result
{
    protected internal Result(T? value, bool isSuccess, string message, string? errorCode = null)
        : base(isSuccess, message, errorCode)
    {
        Value = value;
    }

    public T? Value { get; }
}
