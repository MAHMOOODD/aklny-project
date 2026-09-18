namespace Resturant_Backend.Common.Responses
{
    public class ApiError
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        // for validation errors
        public IDictionary<string, List<string>>? Errors { get; set; }

        public ApiError(int statusCode, string message, IDictionary<string, List<string>>? errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }
    }
}
