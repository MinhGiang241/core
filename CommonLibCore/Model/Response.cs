namespace CommonLibCore.Model
{
    public class Response
    {
        public const int FAILURE = 0;
        public const int SUCCESS = 1;
        public const int NOT_AUTHORIZE = 2;
        public const int EXISTED = 3;
        public const int NOT_EXISTED = 4;

        public const string CODE_SUCCESS = "SUCCESS";
        public const string CODE_FAILURE = "FAIL";

        public Response()
        {
            Status = SUCCESS;
            Code = CODE_SUCCESS;
        }

        public Response(string code, string message)
        {
            Status = SUCCESS;
            Code = code;
            Message = message;
        }

        public Response(int status, string code, string message)
        {
            Status = status;
            Code = code;
            Message = message;
        }

        public Response(int status, string code, string message, object data)
        {
            Status = status;
            Code = code;
            Message = message;
            Data = data;
        }

        public Response(object data)
        {
            Data = data;
        }

        public int Status { get; set; }
        public string Message { get; set; }
        public string Code { get; set; } //more detail error
        public object Data { get; set; } //any type
        public object Extra { get; set; }
    }
}
