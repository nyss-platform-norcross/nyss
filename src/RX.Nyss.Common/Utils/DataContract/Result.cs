namespace RX.Nyss.Common.Utils.DataContract;
using System;

public class Result
{
    public bool IsSuccess { get; set; }

    public Message? Message { get; set; }

    internal Result(bool isSuccess, string? messageKey, object? messageData)
    {
        IsSuccess = isSuccess;
        Message = messageKey is not null
            ? new Message { Key = messageKey, Data = messageData! }
            : null;
    }

    public static Result SuccessMessage(string messageKey, object? messageData = null) => new Result(true, messageKey, messageData);

    public static Result Error(string messageKey, object? messageData = null) => new Result(false, messageKey, messageData);

    public static Result Success() => new Result(true, null, null);

    public static Result<T> Success<T>(T value, string? messageKey = null, object? messageData = null) => new Result<T>(value, true, messageKey, messageData);

    public static Result<T> Error<T>(string messageKey, object? messageData = null) => new Result<T>(default, false, messageKey, messageData);

    public Result<T> Cast<T>()
    {
        if (Message is null || Message.Key is null)
        {
            throw new InvalidOperationException(
                $"Cannot cast a result with no message. Check {nameof(IsSuccess)} before calling {nameof(Cast)}.");
        }

        return Error<T>(Message.Key, Message.Data);
    }

    public override string ToString() => $"{nameof(IsSuccess)}: {IsSuccess}, {nameof(Message)}: {Message}";
}

public class Result<T> : Result
{
    public T? Value { get; }

    public Result(T? value, bool isSuccess, string? messageKey = null, object? messageData = null)
        : base(isSuccess, messageKey, messageData)
    {
        Value = value;
    }
}