
using System.Net;
using System.Reflection;

namespace ExceptionHandlers {
    public abstract class AbstractExceptionHandlerMiddleware
    {
        public static string LocalizationKey => "LocalizationKey";

        private readonly RequestDelegate _next;

        public abstract (HttpStatusCode code, string message) GetResponse(Exception exception);

        public AbstractExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                
                // get the response code and message
                var (status, message) = GetResponse(exception);
                response.StatusCode = (int) status;
                await response.WriteAsync(message);
            }
        }
    }
}