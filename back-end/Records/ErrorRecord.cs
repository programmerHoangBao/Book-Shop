namespace back_end.Records
{
    public sealed record ErrorRecord(string ResponseCode, string Message, int HttpStatus)
    {
        public static readonly ErrorRecord RequestInvalid = new("E001", "Request is invalid!", StatusCodes.Status400BadRequest);
        public static readonly ErrorRecord InternalServerError = new("E002", "Internal server error!", StatusCodes.Status500InternalServerError);
        public static readonly ErrorRecord RequestNotFound = new("E003", "Request not found!", StatusCodes.Status404NotFound);
        public static readonly ErrorRecord UserExists = new("E004", "User is exists!", StatusCodes.Status409Conflict);
    }
}
