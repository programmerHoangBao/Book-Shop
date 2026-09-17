namespace back_end.Records
{
    public sealed record ErrorRecord(string ResponseCode, string Message, int HttpStatus)
    {
    }
}
