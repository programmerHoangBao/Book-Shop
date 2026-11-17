namespace back_end.DTOs.Addresses.Requests
{
    public class CreateAddressRequest : AddressFieldsRequest
    {
        public bool IsDefault { get; set; }
    }
}
