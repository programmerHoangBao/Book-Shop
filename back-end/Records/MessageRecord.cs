namespace back_end.Records
{
    public sealed record MessageRecord(string ResponseCode, string Message, int HttpStatus)
    {
        public static readonly MessageRecord RegisterSuccessfully = 
            new ("S001", "Register is successfully!", StatusCodes.Status200OK);
        public static readonly MessageRecord Success = 
            new("S002", "Successful!", StatusCodes.Status200OK);
        public static readonly MessageRecord CreatedSuccessfully =
            new("S003", "Successfully created!", StatusCodes.Status201Created);
        public static readonly MessageRecord QueuedSuccessfully =
            new("S004", "Notification queued for creation.", StatusCodes.Status202Accepted);
        public static readonly MessageRecord OrderQueuedSuccessfully =
            new("S005", "Order request queued for processing.", StatusCodes.Status202Accepted);
    }
}
