
using System.Data;
using System.Net;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace ExceptionHandlers {

    public class ExceptionHandler: AbstractExceptionHandlerMiddleware {
        
        public ExceptionHandler(RequestDelegate next): base(next)
        {}

        public override ( HttpStatusCode code, string message ) GetResponse(Exception exception) {
            HttpStatusCode code;
            string errorMsg = exception.Message;

            switch (exception) {
                case ArgumentNullException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case SqlException:
                    code = HttpStatusCode.InternalServerError;
                    errorMsg = "Unable to connect to database at this time.";
                    break;
                case (
                    DuplicateNameException or
                    InvalidOperationException or 
                    ArgumentNullException or 
                    ArgumentException
                ):
                    code = HttpStatusCode.BadRequest;
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    errorMsg = "Internal server error";
                    break;
            }
            return (code, JsonConvert.SerializeObject(errorMsg));
        }
    }
}