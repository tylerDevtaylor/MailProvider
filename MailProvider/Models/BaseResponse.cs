namespace MailProvider.Models
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public ErrorResponse Error { get; set; }

        public BaseResponse(T data)
        {
            Data = data;
        }

        public BaseResponse(ErrorResponse error)
        {
            Data = default;
            Error = error;
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public ErrorResponse(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
