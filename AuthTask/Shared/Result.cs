namespace AuthTask.Shared
{
    public class Result
    { 
        protected Result(bool success, string message, int statusCode)
        {
            IsSuccess = success;
            Message = message;
            StatusCode = statusCode;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public string Message { get; }

        public int StatusCode { get; }

        private static readonly Result _success = new(true, string.Empty, 0);

        public static Result Success() => _success;

        public static Result Failure(string message, int statusCode) => new(false, message, statusCode);

        public static Result<T> Success<T>(T data) => new(data, true, string.Empty, 0);

        public static Result<T> Failure<T>(string message, int statusCode) => new(default, false, message, statusCode);
    }

    public class Result<T>(T? data, bool success, string message, int statusCode) : Result(success, message, statusCode)
    {
        private readonly T? _value = data;

        public T Value => IsSuccess ? _value! 
            : throw new InvalidOperationException("The result state is failed, it has no result value.");

        public static implicit operator T(Result<T> result) => result.Value;
    }
}
