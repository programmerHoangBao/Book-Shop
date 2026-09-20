namespace back_end.Records
{
    public sealed record MessageRecord(string ResponseCode, string Message, int HttpStatus)
    {
        public static readonly MessageRecord RegisterSuccessfully = new ("S001", "Register is successfully!", StatusCodes.Status200OK);
    }
}
