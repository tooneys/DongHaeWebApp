namespace WebApi.Models
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public int? TotalCount { get; set; }
        public bool Success { get; set; } = true;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse(string message)
        {
            Message = message;
        }
    }

    public class ApiErrorResponse : ApiResponse<object>
    {
        public string ErrorCode { get; set; }
        public object Details { get; set; }

        public ApiErrorResponse(string message, string errorCode = null, object details = null)
        {
            Message = message;
            Success = false;
            ErrorCode = errorCode;
            Details = details;
        }
    }
}
