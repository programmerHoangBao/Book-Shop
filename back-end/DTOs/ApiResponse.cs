using back_end.Records;

namespace back_end.DTOs
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public int HttpStatus { get; set; }
        public T? Data {  get; set; }

        public static ApiResponse<T> Response
        (
            MessageRecord messageRecord = default,
            ErrorRecord errorRecord = default,
            T? data = default
        )
        {
            if (messageRecord == null)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    ResponseCode = errorRecord.ResponseCode,
                    Message = errorRecord.Message,
                    HttpStatus = errorRecord.HttpStatus,
                    Data = data
                };
            }
            return new ApiResponse<T>
            {
                IsSuccess = true,
                ResponseCode = messageRecord.ResponseCode,
                Message = messageRecord.Message,
                HttpStatus = messageRecord.HttpStatus,
                Data = data
            };
        }
    }
}
