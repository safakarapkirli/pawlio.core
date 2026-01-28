namespace Pawlio.IsyerimPos;

#pragma warning disable CS8618

public interface IResult<T> : IResult
{
    T Content { get; set; }
}

public interface IResult
{
    bool IsDone { get; set; }
    string Message { get; set; }
    int? ErrorCode { get; set; }
    List<Error> Errors { get; set; }
}

public class Result : IResult
{
    public bool IsDone { get; set; }
    public string Message { get; set; }
    public int? ErrorCode { get; set; }
    public List<Error> Errors { get; set; }

    public Result()
    {
        IsDone = true;
        Message = "İşlem Başarılı";
        ErrorCode = 200;
    }

    public Result(IResult result)
    {
        IsDone = result.IsDone;
        Message = result.Message;
        ErrorCode = result.ErrorCode;
        Errors = result.Errors;
    }

    public static Result ToResult(IResult own)
    {
        return new Result
        {
            ErrorCode = own.ErrorCode,
            Errors = own.Errors,
            IsDone = own.IsDone,
            Message = own.Message

        };
    }
}

public class ResultOk : Result
{
    public ResultOk()
    {
    }

    public ResultOk(string message)
    {
        Message = message;
    }

    public ResultOk(IResult result)
    {
        IsDone = result.IsDone;
        Message = result.Message;
        ErrorCode = result.ErrorCode;
        Errors = result.Errors;
    }
}

public class Result<T> : IResult<T>
{
    public string Message { get; set; }
    public int? ErrorCode { get; set; }
    public List<Error> Errors { get; set; }
    public bool IsDone { get; set; }
    public T Content { get; set; }
    public long ElapsedTime { get; set; }

    public Result(T content)
    {
        Message = "İşlem Başarılı";
        ErrorCode = 200;
        IsDone = true;
        Content = content;
    }

    public Result()
    {
        Message = "İşlem Başarılı";
        ErrorCode = 200;
        Content = Content;
        IsDone = true;
    }

    public Result(IResult result)
    {
        Message = result.Message;
        ErrorCode = result.ErrorCode;
        Errors = result.Errors;
        IsDone = result.IsDone;

    }

    public static implicit operator Result<T>(Result r)
    {
        return new Result<T>(r);
    }

    public static Result ToResult(IResult<T> own)
    {
        return new Result
        {
            ErrorCode = own.ErrorCode,
            Errors = own.Errors,
            IsDone = own.IsDone,
            Message = own.Message
        };
    }
}

public class ResultError<T> : Result<T>
{
    public ResultError()
    {
        Message = "İşlem başarısız";
        IsDone = false;
        ErrorCode = 404;
    }

    public ResultError(string message)
    {
        Message = message;
        IsDone = false;
        ErrorCode = 404;
    }

    public ResultError(string mesaage, int errorCode)
    {
        Message = mesaage;
        IsDone = false;
        ErrorCode = errorCode;
    }
}

public class ResultError : Result
{

    public ResultError()
    {
        Message = "İşlem başarısız";
        IsDone = false;
        ErrorCode = 404;
    }

    public ResultError(string message)
    {
        Message = message;
        IsDone = false;
        ErrorCode = 404;
    }

    public ResultError(string mesaage, int errorCode)
    {
        Message = mesaage;
        IsDone = false;
        ErrorCode = errorCode;
    }
}

public class Error
{
    public string Name { get; set; }
    public string ErrorMessage { get; set; }
}
#pragma warning restore CS8618
