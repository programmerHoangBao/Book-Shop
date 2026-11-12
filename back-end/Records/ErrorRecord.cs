namespace back_end.Records;

public sealed record ErrorRecord(
    string ResponseCode,
    string Message,
    int HttpStatus)
{
    public static readonly ErrorRecord RequestInvalid =
        new("E001", "Request is invalid!", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord InternalServerError =
        new("E002", "Internal server error!", StatusCodes.Status500InternalServerError);

    public static readonly ErrorRecord RequestNotFound =
        new("E003", "Request not found!", StatusCodes.Status404NotFound);

    public static readonly ErrorRecord UserExists =
        new("E004", "User already exists!", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord CategoryExists =
        new("E021", "Category already exists!", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord NotFound =
        new("E005", "Data not found!", StatusCodes.Status404NotFound);

    public static readonly ErrorRecord LoginFailed =
        new("E006", "Login failed!", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord OtpExpiry =
        new("E007", "OTP has expired!", StatusCodes.Status403Forbidden);

    public static readonly ErrorRecord Failed =
        new("E008", "Operation failed!", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord RefreshTokenExpiry =
        new("E009", "Refresh token has expired!", StatusCodes.Status403Forbidden);

    public static readonly ErrorRecord UserNotLoggedInLocally =
        new("E010", "User is not logged in locally!", StatusCodes.Status403Forbidden);

    public static readonly ErrorRecord ResetPasswordKeyExpiry =
        new("E011", "Your password reset session has expired!", StatusCodes.Status403Forbidden);

    public static readonly ErrorRecord FileEmpty =
        new("E012", "File cannot be empty!", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord FileTooLarge =
        new("E013", "The file is too large!", StatusCodes.Status413PayloadTooLarge);

    public static readonly ErrorRecord InvalidFileExtension =
        new("E014", "Invalid file extension!", StatusCodes.Status415UnsupportedMediaType);

    public static readonly ErrorRecord UploadFileFailed =
        new("E015", "Failed to upload file!", StatusCodes.Status503ServiceUnavailable);

    public static readonly ErrorRecord UserCreationFailedWithAvatarCleanupFailed =
        new(
            "E016",
            "User creation failed and avatar cleanup could not be completed.",
            StatusCodes.Status500InternalServerError
        );

    public static readonly ErrorRecord Forbidden =
        new("E017", "Access denied.", StatusCodes.Status403Forbidden);

    public static readonly ErrorRecord StoreNotFound =
        new("E018", "Store not found.", StatusCodes.Status404NotFound);

    public static readonly ErrorRecord UserCannotBeRemovedFromStore =
        new("E019", "It is not possible to remove a user from the store.", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord UserCannotBeRestoreFromStore =
        new("E020", "It is not possible to restore a user from the store.", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord BookPurchaseRequired =
        new("E022", "A completed purchase of this book is required to submit a review.", StatusCodes.Status403Forbidden);

    public static readonly ErrorRecord ReviewAlreadyExists =
        new("E023", "A review for this book and order already exists.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord BookUnavailable =
        new("E024", "This book is not available for purchase.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord InsufficientStock =
        new("E025", "The requested quantity exceeds available stock.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord CartEmpty =
        new("E026", "The cart has no books to order.", StatusCodes.Status400BadRequest);

    public static readonly ErrorRecord BookUnavailableForOrder =
        new("E027", "One or more books in the cart are unavailable.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord OrderNotPending =
        new("E028", "Only pending orders can be confirmed.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord OrderNotConfirmed =
        new("E029", "Only confirmed orders can be marked as in transit.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord OrderNotInTransit =
        new("E030", "Only orders in transit can be completed.", StatusCodes.Status409Conflict);

    public static readonly ErrorRecord PromotionCodeExists =
        new("E031", "Promotion code already exists.", StatusCodes.Status409Conflict);
}
