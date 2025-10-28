namespace Pharmacy.Services
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ServiceResult<T> SuccessResult(T data, int statusCode = 200)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ServiceResult<T> ErrorResult(string errorMessage, int statusCode = 400)
        {
            return new ServiceResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }

        public static ServiceResult<T> NotFoundResult(string message = "Resource not found")
        {
            return new ServiceResult<T>
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 404
            };
        }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ServiceResult SuccessResult(int statusCode = 200)
        {
            return new ServiceResult
            {
                Success = true,
                StatusCode = statusCode
            };
        }

        public static ServiceResult ErrorResult(string errorMessage, int statusCode = 400)
        {
            return new ServiceResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }

        public static ServiceResult NotFoundResult(string message = "Resource not found")
        {
            return new ServiceResult
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = 404
            };
        }
    }
}