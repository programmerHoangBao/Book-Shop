# Backend API Description

This document describes the HTTP endpoints implemented by `back-end/Controllers` and their request/response behavior. Routes are relative to the backend host (for example, `https://localhost:<port>`). JSON property names are shown in camelCase; enum values are the enum names defined in `back-end/Enums` unless configured otherwise.

## API configuration and shared behavior

- ASP.NET Core controllers are mapped by `Program.cs`; routes and verbs are declared on controller/action attributes. OpenAPI is mapped only in the Development environment.
- Database, Redis, Kafka, email, Google authentication, S3 file storage, Elasticsearch, JWT authentication and rate limiting are configured by `Program.cs` and `appsettings`/environment configuration. A valid JWT bearer token is sent as `Authorization: Bearer <token>`.
- JWT validation checks issuer, audience, lifetime and signing key. `[Authorize]` requires a valid token; role restrictions are listed for each endpoint. An unauthenticated/invalid-token request can return `401`; a valid identity without the required role can return `403`.
- A global fixed-window limiter applies per client IP to every route. Its permit count is `Security:GlobalRateLimitPermitLimit` requests per one second (no queue). Auth actions may also use a second per-IP policy: `LoginPolicy` uses `Security:LoginRateLimitPermitLimit` per 60 seconds; `OtpPolicy` uses `Security:OtpRateLimitPermitLimit` per `Security:OtpExpirySeconds`. Exceeding a limiter returns `429` JSON `{ "status": 429, "message": "Too many requests. Please try again later." }`, and may include `Retry-After`.
- Most endpoints return the common JSON envelope `{ isSuccess, responseCode, message, httpStatus, data }`. Success codes are `S001` registration, `S002` ordinary success, `S003` created (`201`), `S004` notification queued (`202`), and `S005` order queued (`202`). Errors use `{ isSuccess:false, responseCode, message, httpStatus, data }`. Exact error codes/messages are enumerated below. `data` is endpoint-specific or `null`.
- `[ApiController]` validation failures return `400`, code `E001`, message `Request is invalid!`, and `data` as an array of validation messages. Unhandled `ArgumentException` maps to the same error; `KeyNotFoundException` maps to `404 E003` and includes the exception message in `data`; unhandled exceptions map to `500 E002` with `data:null`.
- File deletion is a `204` response with no body; image upload returns its upload response directly. Framework authentication/authorization failures and the rate-limit response are outside the common envelope.

## Endpoints

### Authentication — `/api/auth`
All endpoints here are public (no bearer token required). Request DTO fields with validation constraints are required as stated. Login, refresh-token and Google sign-in use `LoginPolicy`; register, verify, forgot-password use `OtpPolicy`.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/auth/register` | Start customer registration and send a verification OTP. JSON: `fullName` (required, max 255), `email` (required valid email, max 255), `password` (required; 6–15 chars, upper/lower/digit/special, no spaces). | `200 S001`; `data.otpExpirySeconds`. | `400 E001` invalid input; `409 E004` user exists; `400 E008` operation failed; `429`; `500 E002`. |
| `POST /api/auth/login` | Authenticate local credentials. JSON: `email` (required valid email, max 255), `password` (required; same password format). | `200 S002`; `data.userId`, `accessToken`, `refreshToken`. | `400 E001`; `400 E006` login failed; `404 E005`; `429`; `500 E002`. |
| `POST /api/auth/verify` | Verify OTP for registration/password flow. JSON: `email` (required valid email, max 255), `otp` (required, exactly six digits). | `200 S002`; `data.resetPasswordKey` and `action` (the returned action indicates the verified flow). | `400 E001`; `403 E007` OTP expired; `400 E008`; `404 E005`; `429`; `500 E002`. |
| `POST /api/auth/refresh-token` | Exchange a refresh token for renewed tokens. JSON: `refreshToken` (required). | `200 S002`; updated login/token data. | `400 E001`; `403 E009` token expired; `400 E008`; `404 E005`; `429`; `500 E002`. |
| `POST /api/auth/logout` | Revoke the supplied refresh token. JSON: `refreshToken` (required). | `200 S002`, normally `data:null`. | `400 E001`; `403 E010` no local login; `404 E005`; `400 E008`; `500 E002`. |
| `POST /api/auth/google-sign-in` | Authenticate using a Google ID token. Query: `idToken` (required). | `200 S002`; login/token data. | `400 E001` or Google-token/operation failure; `404 E005`; `429`; `500 E002`. |
| `POST /api/auth/forgot-password` | Begin password recovery and issue/send OTP. JSON: `email` (required valid email, max 255). | `200 S002`; `data` is the forgot-password result. | `400 E001`; `404 E005`; `429`; `500 E002`. The service avoids exposing account details beyond its returned result. |
| `PATCH /api/auth/reset-password` | Set a new password using the reset key. JSON: `resetPasswordKey` (required GUID), `newPassword` (required; 6–15 chars, upper/lower/digit/special, no spaces). | `200 S002`, normally `data:null`. | `400 E001`; `403 E011` reset session expired; `403 E010` user is not locally registered; `404 E005`; `400 E008`; `500 E002`. |

### Books — `/api/books`
Public reads do not require authentication. Book creation/import, inventory changes, update, delete, restore and deleted-book reads require `Admin` or `Manager`.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/books/import/new` | Import file rows as new books and inventory. Multipart form: `file` (spreadsheet/import file), optional `storeId` (GUID). | Import result in `data` (accepted/rejected row results). | `400 E001`/import validation errors; `404 E005` or `E018`; `403 E017`; `413 E013`; `415 E014`; `503 E015`; `500 E002`; `401`/`403` role failure; `429`. |
| `POST /api/books/import/existing` | Import inventory quantities for existing books. Multipart: `file`; optional `storeId`. | Import result in `data`. | Same import errors as `import/new`. |
| `POST /api/books/inventory/new` | Create a book and add initial stock. JSON: optional `storeId`; `name` (required, max 500); optional `author`/`publisher` (max 255), `description`; `price` (>=0); `categoryId`; `imageUrls` (string array); `quantity` (>0). | `201 S003`; created book/inventory data. | `400 E001`; `404 E005`/`E018`; `403 E017`; `409 E024`/`E025` where applicable; `500 E002`; `401`/`403`; `429`. |
| `POST /api/books/{bookId}/inventory` | Add stock to an existing book. Path: `bookId` GUID. JSON: optional `storeId`; `quantity` (>0). | `200 S002`; inventory update data. | `400 E001`; `404 E005`/`E018`; `403 E017`; `409 E025`; `500 E002`; `401`/`403`; `429`. |
| `PUT /api/books/{bookId}` | Update book metadata. Path: `bookId` GUID. JSON: `name` required (max 500), optional `author`/`publisher` (max 255), optional `description`, `price` (>=0), `categoryId` required, `imageUrls` array. | `200 S002`; updated book data. | `400 E001`; `404 E005`; `403 E017`; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/books/{bookId}` | Soft-delete a book. Path: `bookId` GUID. | `200 S002`, typically no data. | `400 E001`; `404 E005`; `403 E017`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `PATCH /api/books/{bookId}/restore` | Restore a soft-deleted book. Path: `bookId` GUID. | `200 S002`, typically no data. | `400 E001`; `404 E005`; `403 E017`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/books/{bookId}` | Get a book detail. Path: `bookId` GUID. | `200 S002`; book detail in `data`. | `404 E005`; `400 E001`; `500 E002`; `429`. |
| `GET /api/books/{bookId}/promotions` | Get eligible promotion suggestions for a book. Path: `bookId`; query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paged promotion suggestions. | `400 E001`; `404 E005`; `500 E002`; `429`. |
| `GET /api/books` | Cursor-page active/public books. Query: `cursor` (optional), `pageSize` (default 10). | `200 S002`; `data.items` plus `data.nextCursor`. | `400 E001`; `500 E002`; `429`. |
| `GET /api/books/deleted` | Page through deleted books. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated book data. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/books/deleted/search` | Search deleted books. Query DTO: optional `key`, optional `book` status, optional `isDeleted`, `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated matching books. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/books/search` | Search active books. Query DTO: optional `key`, optional `book` status, optional `isDeleted`, `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated matching books. | `400 E001`; `404 E005`; `500 E002`; `429`. |

### Categories — `/api/category`
Read active category endpoints are public. Create/update/delete/restore and deleted-category listing require `Admin`.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/category` | Create category. JSON: `name` (required, max 150), optional `parentId` GUID, `isActive` (default true). | `201 S003`; category data. | `400 E001`; `409 E021`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `PUT /api/category` | Update category. JSON: `categoryId` GUID, `name` (required, max 150), optional `parentId`, `isActive` (default true). | `200 S002`; category data. | `400 E001`; `404 E005`; `409 E021`; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/category/{categoryId}` | Soft-delete category. Path: GUID. | `200 S002`, `data:null`. | `400 E001`; `404 E005`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `PATCH /api/category/{categoryId}/restore` | Restore deleted category. Path: GUID. | `200 S002`, `data:null`. | `400 E001`; `404 E005`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/category/{categoryId}` | Get category by ID. Path: GUID. | `200 S002`; category data. | `404 E005`; `400 E001`; `500 E002`; `429`. |
| `GET /api/category` | List active categories. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; pagination object with `items`, `currentPage`, `pageSize`, `totalItems`, `totalPages`. | `400 E001`; `404 E005` when no result; `500 E002`; `429`. |
| `GET /api/category/deleted` | List deleted categories. Same pagination query. | `200 S002`; pagination object. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |

### Stores and inventory
Store read/search endpoints are public. Store administration requires `Admin`. Inventory listing requires `Admin`, `Manager`, or `Employee`; the service may additionally enforce store assignment.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/store` | Create store. JSON: `address` (required, max 500), `phoneNumber` (max 11). | `201 S003`; store data. | `400 E001`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `PUT /api/store` | Update store. JSON: `storeId` GUID, `address` (required, max 500). | `200 S002`; updated store. | `400 E001`; `404 E005`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/store?Id={guid}` | Soft-delete store. Query: `Id` (required GUID). | `200 S002`, `data:null`. | `400 E001`; `404 E005`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `PATCH /api/store?Id={guid}` | Restore store. Query: `Id` (required GUID). | `200 S002`, `data:null`. | `400 E001`; `404 E005`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/store` | List stores. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated store data. | `400 E001`; `404 E005`; `500 E002`; `429`. |
| `GET /api/store/{Id}` | Get store by ID. Path: `Id` GUID. | `200 S002`; store data. | `400 E001`; `404 E005`; `500 E002`; `429`. |
| `GET /api/store/deleted` | List deleted stores. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated store data. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/store/search` | Search stores by key and deletion flag. Query: optional `key`, `isDeleted` (default true), `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; pagination object with store items. | `400 E001`; `404 E005`; `500 E002`; `429`. |
| `GET /api/store/{storeId}/books` | List inventory for store. Path: `storeId` GUID; query `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paged inventory book records. | `400 E001`; `404 E018` store missing; `403 E017` access denied/unassigned store; `500 E002`; `401`/`403`; `429`. |

### Users — `/api/user`
Profile get/update endpoints require any authenticated user; service-level ownership checks apply. Create requires `Manager` or `Admin`. Delete/restore operations require `Manager`, `Admin`, or `Employee` and may be further restricted by store/role rules.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/user` | Create staff account. Request DTO fields: full name, email, password, role, optional phone/avatar URL and store ID; email/password validation follows registration constraints. The action declares `multipart/form-data` but binds the DTO with `[FromBody]`; this mismatch can affect model binding. | `201 S003`; created user. | `400 E001`; `409 E004`; `404 E018`; `403 E017`; `400 E008`; `415` if the declared content type is not accepted; `500 E002`/`E016`; `401`/`403`; `429`. |
| `GET /api/user?Id={guid}` | Get user profile. Query: `Id` GUID. | `200 S002`; profile data. | `400 E001`; `404 E005`; `403 E017`; `500 E002`; `401`; `429`. |
| `GET /api/user/email?email={email}` | Find profile by email. Query: `email` (required). | `200 S002`; profile data. | `400 E001`; `404 E005`; `403 E017`; `500 E002`; `401`; `429`. |
| `PUT /api/user` | Update profile. JSON: `id` GUID, `fullName` (required, max 255), `email` (required valid email, max 255). | `200 S002`; updated profile. | `400 E001`; `404 E005`; `403 E017`; `409 E004`; `500 E002`; `401`; `429`. |
| `DELETE /api/user?Id={guid}` | Soft-delete a user. Query: `Id` GUID. | `200 S002`; result data. | `400 E001`; `404 E005`/`E003`; `403 E017`; `400 E019`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `PATCH /api/user/restore?Id={guid}` | Restore user. Query: `Id` GUID. | `200 S002`; result data. | `400 E001`; `404 E005`/`E003`; `403 E017`; `400 E020`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/user/bulk` | Soft-delete users. JSON: `userIds` array of GUIDs. | `200 S002`; per-user successes/errors. | `400 E001`; `404 E003`; `403 E017`; `400 E019`; `500 E002`; `401`/`403`; `429`. |
| `PATCH /api/user/bulk/restore` | Restore users. JSON: `userIds` array of GUIDs. | `200 S002`; per-user successes/errors. | `400 E001`; `404 E003`; `403 E017`; `400 E020`; `500 E002`; `401`/`403`; `429`. |

### Cart — `/api/cart`
All cart routes require a `Customer` bearer token; the cart is associated with the current customer.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `GET /api/cart/items` | Get paginated cart contents. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paged items (`bookId`, `bookName`, `quantity`, `unitPrice`). | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/cart/items` | Add book to cart. JSON: `bookId` GUID, `quantity` (default 1; >=1). | `200 S002`; cart item/result. | `400 E001`; `404 E005`; `409 E024` unavailable or `E025` insufficient stock; `500 E002`; `401`/`403`; `429`. |
| `PUT /api/cart/items/{bookId}` | Change cart quantity. Path: `bookId` GUID. JSON: `quantity` (>=1). | `200 S002`; updated cart item. | `400 E001`; `404 E005`; `409 E024`/`E025`; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/cart/items/{bookId}` | Remove book from cart. Path: `bookId` GUID. | `200 S002`; result, usually no data. | `400 E001`; `404 E005`; `400 E008`; `500 E002`; `401`/`403`; `429`. |

### Orders and invoices
Order summary requires `Admin` or `Manager`. Placing/cancelling orders and viewing invoices require `Customer`. Confirming, shipping and completing orders require `Admin`, `Manager`, or `Employee`.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `GET /api/orders/summary` | Count non-deleted orders by status; managers are scoped to assigned stores. No parameters. | `200 S002`; `newOrders`, `inProgressOrders`, `completedOrders`, `cancelledOrders`. | `403 E017`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/orders` | Submit current cart as an order. JSON: `addressId` GUID; `paymentMethod` enum (default `CashOnDelivery`); optional `promotionCode` (max 6). Optional `Idempotency-Key` header. | Typically `202 S005` when queued, with order result data; some code paths return `200 S002`. | `400 E001`/`E026` empty cart; `403 E017`; `404 E005`; `409 E024`/`E025`/`E027`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/orders/{orderId}/confirm` | Confirm a pending order. Path: `orderId` GUID. | `200 S002`; confirmation data. | `400 E001`; `404 E005`; `403 E017`; `409 E028`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/orders/{orderId}/in-transit` | Mark confirmed order in transit. Path: `orderId` GUID. | `200 S002`; transit data. | `400 E001`; `404 E005`; `403 E017`; `409 E029`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/orders/{orderId}/complete` | Complete an in-transit order. Path: `orderId` GUID. | `200 S002`; completion data. | `400 E001`; `404 E005`; `403 E017`; `409 E030`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/orders/{orderId}/cancel` | Cancel the current customer's pending/confirmed order. Path: `orderId` GUID. | `200 S002`; cancellation data. | `400 E001`; `404 E005`; `403 E017` if not owned; `409 E032`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/invoices` | List current customer's invoices filtered by order status. Query: `status` (`OrderStatus`, required); `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paged invoices including totals, payment/order status and line items. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |

### Returns
Customers create and list their own returns; staff roles confirm returns. The service checks ownership, order state and return-window rules.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/orders/{orderId}/returns` | Request return for a completed order. Path: `orderId` GUID. JSON: `bankAccountName` (required, max 255), `bankAccountNumber` (required, max 30), `bankName` (required, max 255), `reason` (required, max 1000), `items` (at least one `{ orderItemId: GUID, quantity: int }`), `evidenceImageUrls` (at least one string). | `201 S003`; return request data. | `400 E001`/`E036`; `403 E017`; `404 E005`; `409 E033` incomplete order, `E034` window expired, `E035` duplicate; `500 E002`; `401`/`403`; `429`. |
| `GET /api/returns` | List current customer's return requests. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated return request records. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `POST /api/returns/{returnRequestId}/confirm` | Confirm/process a pending return request. Path: `returnRequestId` GUID. | `200 S002`; confirmation result. | `400 E001`; `404 E005`; `403 E017`; `409 E037` request not pending; `500 E002`; `401`/`403`; `429`. |

### Reviews — `/api/reviews`
Creating a review requires a valid bearer token. Public review listing is explicitly anonymous. Delete/restore/deleted listing requires `Admin` or `Manager`.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/reviews` | Review a purchased book. JSON: `bookId` and `orderId` GUIDs, `rating` 1–5, optional `comment`, optional `imageUrls` array. | `201 S003`; review data. | `400 E001`; `403 E017` or `E022` purchase required; `404 E005`; `409 E023` duplicate; `500 E002`; `401`/`403`; `429`. |
| `GET /api/reviews` | Get public paginated reviews and rating summary for a book. Query: `bookId` GUID; `pageNumber` (default 1); `pageSize` (default 10). | `200 S002`; `ratingSummary` (average, count, distribution) and paged review items. | `400 E001`; `404 E005`; `500 E002`; `429`. |
| `GET /api/reviews/deleted` | List deleted reviews. Query: `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated deleted reviews. | `400 E001`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/reviews/{reviewId}` | Soft-delete review. Path: `reviewId` GUID. | `200 S002`, usually `data:null`. | `400 E001`; `404 E005`; `403 E017`; `400 E008`; `500 E002`; `401`/`403`; `429`. |
| `PATCH /api/reviews/{reviewId}/restore` | Restore review. Path: `reviewId` GUID. | `200 S002`, usually `data:null`. | `400 E001`; `404 E005`; `403 E017`; `400 E008`; `500 E002`; `401`/`403`; `429`. |

### Promotions — `/api/promotions`
All routes require an `Admin` bearer token.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/promotions` | Create promotion. JSON: optional `code` (max 6), `name` (required, max 255), `type` (`PromotionType`), `discountValue`, `minimumOrderValue`, `startAt`, `endAt`, `usageLimit`, `freeShipping`, `isActive` (default true), `bookIds` and `categoryIds` GUID arrays. | `201 S003`; promotion data. | `400 E001`/`E008`; `409 E031` duplicate promotion code; `404 E005`; `500 E002`; `401`/`403`; `429`. |

### Notifications — `/api/notifications`
A valid bearer token is required to read notifications; creation requires `Admin` or `Manager`. The controller accepts a target `userId`; whether the caller can access that target is checked by the service.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/notifications` | Queue notification creation. JSON: `userId` GUID, `type` (`NotificationType`), `title` (required, max 500), `content` (required), `channel` (`NotificationChannel`, default `InApp`). | `202 S004`; `data.notificationId`, `status` (Queued). | `400 E001`; `403 E017`; `404 E005`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/notifications/user/{userId}` | Get paged notifications for a user. Path: `userId` GUID; query `pageNumber` (default 1), `pageSize` (default 10). | `200 S002`; paginated notifications (`id`, user, type, title, content, channel, read state, sent/created times). | `400 E001`; `403 E017`; `404 E005`; `500 E002`; `401`/`403`; `429`. |

### Images — `/api/image`
Both endpoints require any authenticated bearer token. Actual files are stored using the configured S3/file-storage service; upload limits and allowed file types are enforced by the service.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `POST /api/image` | Upload an image. Multipart form field: optional `file` (`IFormFile`; service rejects missing/empty files). | `200` direct JSON object: `objectKey`, `imageUrl`, `contentType`, `sizeBytes`. | `400 E012` empty file; `413 E013` too large; `415 E014` unsupported extension; `503 E015` storage failure; `500 E002`; `401`/`403`; `429`. |
| `DELETE /api/image?objectKey={key}` | Delete the stored image. Query: `objectKey` (required). | `204 No Content`, no body. | `400 E001`; storage/service errors (`503 E015` or `500 E002`); `401`/`403`; `429`. |

### Revenue reports — `/api/revenue`
Requires `Admin` or `Manager`. Managers are scoped to their assigned stores; requesting an unassigned `storeId` returns `403 E038`. Revenue uses completed, paid order line totals and store allocation from order inventory reservations. Date range uses inclusive `startDate` and exclusive `endDate`; the filters are optional.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `GET /api/revenue/by-period` | Group revenue by time bucket. Query: `period` (`day`, `week`, `month`, `quarter`, `year`; default `month`), optional `startDate`, `endDate`, `storeId` GUID. | `200 S002`; report with range, period, total revenue, order count and items (`key`, `name`, `periodStart`, revenue, order count, quantity sold). | `400 E001` invalid period/range; `403 E038`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/revenue/by-store` | Group revenue by store. Query: optional `startDate`, `endDate`, `storeId`. | `200 S002`; revenue report with store items. | `400 E001`; `403 E038`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/revenue/by-product` | Group revenue by product. Same optional query fields. | `200 S002`; revenue report with product items. | `400 E001`; `403 E038`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/revenue/by-category` | Group revenue by category. Same optional query fields. A book in multiple categories contributes to each category. | `200 S002`; revenue report with category items. | `400 E001`; `403 E038`; `500 E002`; `401`/`403`; `429`. |

### Merchandise reports — `/api/merchandise/reports`
Requires `Admin` or `Manager`; managers are limited to inventory in active assigned stores. Report rows include `bookId`, `name`, `author`, `availableQuantity` (null for best sellers), and `quantitySold`.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `GET /api/merchandise/reports/low-stock` | Find books with positive available stock up to threshold. Query: `threshold` (default 5; valid 0–1,000,000). | `200 S002`; array of report rows. | `400 E001`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/merchandise/reports/out-of-stock` | Find books with zero/negative stock. No parameters. | `200 S002`; array of report rows. | `500 E002`; `401`/`403`; `429`. |
| `GET /api/merchandise/reports/best-selling` | Top books by completed-order units sold in trailing period. Query: `periodDays` (default 30; valid 1–3,650), `limit` (default 10; valid 1–100). | `200 S002`; array of report rows. | `400 E001`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/merchandise/reports/slow-moving` | Inventory books with low unit sales in trailing period. Query: `periodDays` (default 30; valid 1–3,650), `maximumUnitsSold` (default 5; valid 0–1,000,000). | `200 S002`; array of report rows. | `400 E001`; `500 E002`; `401`/`403`; `429`. |

### Customer reports — `/api/customer-reports`
Requires `Admin`. Dates are UTC timestamps; `startDate` is inclusive and `endDate` exclusive. When both are omitted, the service uses the trailing 30 days; if one is omitted, it derives the other from the supplied bound/current UTC time. Start must precede end.

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `GET /api/customer-reports/new-customers` | Count non-deleted customer accounts created in range. Query: optional `startDate`, `endDate`. | `200 S002`; `{ startDate, endDate, customerCount }`. | `400 E001` invalid range/date; `500 E002`; `401`/`403`; `429`. |
| `GET /api/customer-reports/returning-customers` | Count customers with a completed purchase in range and one before it. Same optional date query. | `200 S002`; `{ startDate, endDate, customerCount }`. | `400 E001`; `500 E002`; `401`/`403`; `429`. |
| `GET /api/customer-reports/purchase-frequency` | Summarize completed orders per customer in range. Same optional date query. | `200 S002`; effective range, `totalCompletedOrders`, `purchasingCustomerCount`, `averagePurchasesPerCustomer`, `frequencyDistribution` (`completedOrderCount`, `customerCount`). | `400 E001`; `500 E002`; `401`/`403`; `429`. |

### Other route — weather forecast

| Method and route | Function and input | Success output | Other possible responses |
|---|---|---|---|
| `GET /WeatherForecast` | Sample framework forecast endpoint. No input and no authorization attribute. | `200` raw JSON array of five forecast objects: `date`, `temperatureC`, `temperatureF`, `summary`. | `429` global limiter; `500` unexpected server failure. |

## Error code reference

| Code | HTTP | Message | Meaning |
|---|---:|---|---|
| `E001` | 400 | Request is invalid! | Validation or invalid request. |
| `E002` | 500 | Internal server error! | Unexpected exception. |
| `E003` | 404 | Request not found! | Missing key/request resource. |
| `E004` | 409 | User already exists! | Duplicate user. |
| `E005` | 404 | Data not found! | Requested data was not found. |
| `E006` | 400 | Login failed! | Invalid login. |
| `E007` | 403 | OTP has expired! | OTP expired. |
| `E008` | 400 | Operation failed! | Operation could not be completed. |
| `E009` | 403 | Refresh token has expired! | Refresh token expired. |
| `E010` | 403 | User is not logged in locally! | Local auth state is unavailable. |
| `E011` | 403 | Your password reset session has expired! | Reset session expired. |
| `E012` | 400 | File cannot be empty! | Missing or empty upload. |
| `E013` | 413 | The file is too large! | Upload exceeds configured size. |
| `E014` | 415 | Invalid file extension! | Unsupported upload type. |
| `E015` | 503 | Failed to upload file! | Storage upload failed. |
| `E016` | 500 | User creation failed and avatar cleanup could not be completed. | User creation and cleanup both failed. |
| `E017` | 403 | Access denied. | Service-level authorization/ownership denial. |
| `E018` | 404 | Store not found. | Store missing. |
| `E019` | 400 | It is not possible to remove a user from the store. | User deletion/store constraint. |
| `E020` | 400 | It is not possible to restore a user from the store. | User restoration/store constraint. |
| `E021` | 409 | Category already exists! | Duplicate category. |
| `E022` | 403 | A completed purchase of this book is required to submit a review. | Review purchase requirement failed. |
| `E023` | 409 | A review for this book and order already exists. | Duplicate review. |
| `E024` | 409 | This book is not available for purchase. | Book unavailable. |
| `E025` | 409 | The requested quantity exceeds available stock. | Insufficient stock. |
| `E026` | 400 | The cart has no books to order. | Empty cart. |
| `E027` | 409 | One or more books in the cart are unavailable. | Cart includes unavailable books. |
| `E028` | 409 | Only pending orders can be confirmed. | Invalid order state. |
| `E029` | 409 | Only confirmed orders can be marked as in transit. | Invalid order state. |
| `E030` | 409 | Only orders in transit can be completed. | Invalid order state. |
| `E031` | 409 | Promotion code already exists. | Duplicate promotion. |
| `E032` | 409 | Only pending or confirmed orders can be cancelled. | Invalid order state. |
| `E033` | 409 | Return requests are only allowed for completed orders. | Order is not complete. |
| `E034` | 409 | The return request window has expired. | Return period elapsed. |
| `E035` | 409 | A return request already exists for this order. | Duplicate return. |
| `E036` | 400 | The requested return items or quantities are invalid. | Invalid return detail. |
| `E037` | 409 | Only pending return requests can be confirmed. | Invalid return state. |
| `E038` | 403 | You are not assigned to the requested store. | Manager lacks store assignment. |

> Error lists above summarize known service and middleware branches. Some infrastructure failures can surface as `E002`; authorization challenges and rate-limit rejections use framework response formats. Response bodies and status codes should be treated as implementation behavior and may change with configuration.

## Request examples for every endpoint

Examples use illustrative IDs and data. Query examples can be appended to the route as written. JSON bodies use camelCase. A body of `—` means no request body is sent.

### Authentication

| API | Example input |
|---|---|
| `POST /api/auth/register` | `{"fullName":"Ari Reader","email":"ari@example.com","password":"Read!234"}` |
| `POST /api/auth/login` | `{"email":"ari@example.com","password":"Read!234"}` |
| `POST /api/auth/verify` | `{"email":"ari@example.com","otp":"123456"}` |
| `POST /api/auth/refresh-token` | `{"refreshToken":"eyJ...refresh-token..."}` |
| `POST /api/auth/logout` | `{"refreshToken":"eyJ...refresh-token..."}` |
| `POST /api/auth/google-sign-in` | `POST /api/auth/google-sign-in?idToken=eyJ...google-id-token...` |
| `POST /api/auth/forgot-password` | `{"email":"ari@example.com"}` |
| `PATCH /api/auth/reset-password` | `{"resetPasswordKey":"550e8400-e29b-41d4-a716-446655440000","newPassword":"NewRead!234"}` |

### Books and categories

| API | Example input |
|---|---|
| `POST /api/books/import/new` | Multipart form fields: `file=@books.xlsx`, `storeId=550e8400-e29b-41d4-a716-446655440000` |
| `POST /api/books/import/existing` | Multipart form fields: `file=@inventory.xlsx`, `storeId=550e8400-e29b-41d4-a716-446655440000` |
| `POST /api/books/inventory/new` | `{"storeId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","publisher":"North Press","description":"A sample book","price":19.99,"categoryId":"550e8400-e29b-41d4-a716-446655440001","imageUrls":["https://cdn.example.com/book.jpg"],"quantity":12}` |
| `POST /api/books/{bookId}/inventory` | Path `bookId=550e8400-e29b-41d4-a716-446655440000`; `{"storeId":"550e8400-e29b-41d4-a716-446655440001","quantity":5}` |
| `PUT /api/books/{bookId}` | Path `bookId=550e8400-e29b-41d4-a716-446655440000`; `{"name":"The Example, Revised","author":"A. Writer","publisher":"North Press","description":"Updated description","price":21.5,"categoryId":"550e8400-e29b-41d4-a716-446655440001","imageUrls":[]}` |
| `DELETE /api/books/{bookId}` | `DELETE /api/books/550e8400-e29b-41d4-a716-446655440000` (no body) |
| `PATCH /api/books/{bookId}/restore` | `PATCH /api/books/550e8400-e29b-41d4-a716-446655440000/restore` (no body) |
| `GET /api/books/{bookId}` | `GET /api/books/550e8400-e29b-41d4-a716-446655440000` |
| `GET /api/books/{bookId}/promotions` | `GET /api/books/550e8400-e29b-41d4-a716-446655440000/promotions?pageNumber=1&pageSize=10` |
| `GET /api/books` | `GET /api/books?pageSize=10` (next page example: `?cursor=eyJ...&pageSize=10`) |
| `GET /api/books/deleted` | `GET /api/books/deleted?pageNumber=1&pageSize=10` |
| `GET /api/books/deleted/search` | `GET /api/books/deleted/search?key=history&isDeleted=true&pageNumber=1&pageSize=10` |
| `GET /api/books/search` | `GET /api/books/search?key=history&book=Available&isDeleted=false&pageNumber=1&pageSize=10` |
| `POST /api/category` | `{"name":"History","parentId":null,"isActive":true}` |
| `PUT /api/category` | `{"categoryId":"550e8400-e29b-41d4-a716-446655440000","name":"World History","parentId":null,"isActive":true}` |
| `DELETE /api/category/{categoryId}` | `DELETE /api/category/550e8400-e29b-41d4-a716-446655440000` (no body) |
| `PATCH /api/category/{categoryId}/restore` | `PATCH /api/category/550e8400-e29b-41d4-a716-446655440000/restore` (no body) |
| `GET /api/category/{categoryId}` | `GET /api/category/550e8400-e29b-41d4-a716-446655440000` |
| `GET /api/category` | `GET /api/category?pageNumber=1&pageSize=10` |
| `GET /api/category/deleted` | `GET /api/category/deleted?pageNumber=1&pageSize=10` |

### Stores and users

| API | Example input |
|---|---|
| `POST /api/store` | `{"address":"12 Main Street","phoneNumber":"0123456789"}` |
| `PUT /api/store` | `{"storeId":"550e8400-e29b-41d4-a716-446655440000","address":"20 Main Street"}` |
| `DELETE /api/store?Id={guid}` | `DELETE /api/store?Id=550e8400-e29b-41d4-a716-446655440000` (no body) |
| `PATCH /api/store?Id={guid}` | `PATCH /api/store?Id=550e8400-e29b-41d4-a716-446655440000` (no body) |
| `GET /api/store` | `GET /api/store?pageNumber=1&pageSize=10` |
| `GET /api/store/{Id}` | `GET /api/store/550e8400-e29b-41d4-a716-446655440000` |
| `GET /api/store/deleted` | `GET /api/store/deleted?pageNumber=1&pageSize=10` |
| `GET /api/store/search` | `GET /api/store/search?key=Main&isDeleted=false&pageNumber=1&pageSize=10` |
| `GET /api/store/{storeId}/books` | `GET /api/store/550e8400-e29b-41d4-a716-446655440000/books?pageNumber=1&pageSize=10` |
| `POST /api/user` | Intended fields: `fullName=Sam Staff`, `email=sam@example.com`, `password=Staff!234`, `role=Employee`, `phoneNumber=0123456789`, `storeId=550e8400-e29b-41d4-a716-446655440000`. The controller declares multipart content while using a body-bound DTO, so verify the accepted wire format against the deployed build. |
| `GET /api/user?Id={guid}` | `GET /api/user?Id=550e8400-e29b-41d4-a716-446655440000` |
| `GET /api/user/email` | `GET /api/user/email?email=sam%40example.com` |
| `PUT /api/user` | `{"id":"550e8400-e29b-41d4-a716-446655440000","fullName":"Sam Reader","email":"sam@example.com"}` |
| `DELETE /api/user?Id={guid}` | `DELETE /api/user?Id=550e8400-e29b-41d4-a716-446655440000` (no body) |
| `PATCH /api/user/restore?Id={guid}` | `PATCH /api/user/restore?Id=550e8400-e29b-41d4-a716-446655440000` (no body) |
| `DELETE /api/user/bulk` | `{"userIds":["550e8400-e29b-41d4-a716-446655440000","550e8400-e29b-41d4-a716-446655440001"]}` |
| `PATCH /api/user/bulk/restore` | `{"userIds":["550e8400-e29b-41d4-a716-446655440000"]}` |

### Cart, orders, invoices and returns

| API | Example input |
|---|---|
| `GET /api/cart/items` | `GET /api/cart/items?pageNumber=1&pageSize=10` |
| `POST /api/cart/items` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","quantity":2}` |
| `PUT /api/cart/items/{bookId}` | Path `bookId=550e8400-e29b-41d4-a716-446655440000`; `{"quantity":3}` |
| `DELETE /api/cart/items/{bookId}` | `DELETE /api/cart/items/550e8400-e29b-41d4-a716-446655440000` (no body) |
| `GET /api/orders/summary` | `GET /api/orders/summary` |
| `POST /api/orders` | Header `Idempotency-Key: checkout-20261114-001`; `{"addressId":"550e8400-e29b-41d4-a716-446655440000","paymentMethod":"CashOnDelivery","promotionCode":"SAVE10"}` |
| `POST /api/orders/{orderId}/confirm` | `POST /api/orders/550e8400-e29b-41d4-a716-446655440000/confirm` (no body) |
| `POST /api/orders/{orderId}/in-transit` | `POST /api/orders/550e8400-e29b-41d4-a716-446655440000/in-transit` (no body) |
| `POST /api/orders/{orderId}/complete` | `POST /api/orders/550e8400-e29b-41d4-a716-446655440000/complete` (no body) |
| `POST /api/orders/{orderId}/cancel` | `POST /api/orders/550e8400-e29b-41d4-a716-446655440000/cancel` (no body) |
| `GET /api/invoices` | `GET /api/invoices?status=Completed&pageNumber=1&pageSize=10` |
| `POST /api/orders/{orderId}/returns` | `{"bankAccountName":"Ari Reader","bankAccountNumber":"1234567890","bankName":"Example Bank","reason":"Damaged cover","items":[{"orderItemId":"550e8400-e29b-41d4-a716-446655440002","quantity":1}],"evidenceImageUrls":["https://cdn.example.com/evidence.jpg"]}` |
| `GET /api/returns` | `GET /api/returns?pageNumber=1&pageSize=10` |
| `POST /api/returns/{returnRequestId}/confirm` | `POST /api/returns/550e8400-e29b-41d4-a716-446655440000/confirm` (no body) |

### Reviews, promotions, notifications, images and reports

| API | Example input |
|---|---|
| `POST /api/reviews` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","orderId":"550e8400-e29b-41d4-a716-446655440001","rating":5,"comment":"Excellent read.","imageUrls":[]}` |
| `GET /api/reviews` | `GET /api/reviews?bookId=550e8400-e29b-41d4-a716-446655440000&pageNumber=1&pageSize=10` |
| `GET /api/reviews/deleted` | `GET /api/reviews/deleted?pageNumber=1&pageSize=10` |
| `DELETE /api/reviews/{reviewId}` | `DELETE /api/reviews/550e8400-e29b-41d4-a716-446655440000` (no body) |
| `PATCH /api/reviews/{reviewId}/restore` | `PATCH /api/reviews/550e8400-e29b-41d4-a716-446655440000/restore` (no body) |
| `POST /api/promotions` | `{"code":"SAVE10","name":"Ten percent off","type":"PercentageDiscount","discountValue":10,"minimumOrderValue":50,"startAt":"2026-11-14T00:00:00Z","endAt":"2026-12-14T00:00:00Z","usageLimit":100,"freeShipping":false,"isActive":true,"bookIds":[],"categoryIds":[]}` |
| `POST /api/notifications` | `{"userId":"550e8400-e29b-41d4-a716-446655440000","type":"Order","title":"Order confirmed","content":"Your order is confirmed.","channel":"InApp"}` |
| `GET /api/notifications/user/{userId}` | `GET /api/notifications/user/550e8400-e29b-41d4-a716-446655440000?pageNumber=1&pageSize=10` |
| `POST /api/image` | Multipart form field `file=@cover.jpg` |
| `DELETE /api/image?objectKey={key}` | `DELETE /api/image?objectKey=books%2Fcover-123.jpg` (no body) |
| `GET /api/revenue/by-period` | `GET /api/revenue/by-period?period=month&startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z&storeId=550e8400-e29b-41d4-a716-446655440000` |
| `GET /api/revenue/by-store` | `GET /api/revenue/by-store?startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z` |
| `GET /api/revenue/by-product` | `GET /api/revenue/by-product?startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z` |
| `GET /api/revenue/by-category` | `GET /api/revenue/by-category?startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z` |
| `GET /api/merchandise/reports/low-stock` | `GET /api/merchandise/reports/low-stock?threshold=5` |
| `GET /api/merchandise/reports/out-of-stock` | `GET /api/merchandise/reports/out-of-stock` |
| `GET /api/merchandise/reports/best-selling` | `GET /api/merchandise/reports/best-selling?periodDays=30&limit=10` |
| `GET /api/merchandise/reports/slow-moving` | `GET /api/merchandise/reports/slow-moving?periodDays=30&maximumUnitsSold=5` |
| `GET /api/customer-reports/new-customers` | `GET /api/customer-reports/new-customers?startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z` |
| `GET /api/customer-reports/returning-customers` | `GET /api/customer-reports/returning-customers?startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z` |
| `GET /api/customer-reports/purchase-frequency` | `GET /api/customer-reports/purchase-frequency?startDate=2026-10-01T00%3A00%3A00Z&endDate=2026-11-01T00%3A00%3A00Z` |
| `GET /WeatherForecast` | `GET /WeatherForecast` |

## Response examples

All object property names in these examples are illustrative and follow the API's JSON camel-case serialization. IDs, dates and values are sample data. For each route, the success and error status/code possibilities are listed in the endpoint catalog above; the examples below show the body format to expect for each response family.

### Common successful response envelope

Successful API responses that use `ApiResponse<T>` contain the corresponding success code from the route catalog and a route-specific `data` value. For example, a profile lookup might return:

```json
{
  "isSuccess": true,
  "responseCode": "S002",
  "message": "Successful!",
  "httpStatus": 200,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "ari@example.com",
    "fullName": "Ari Reader",
    "role": "Customer"
  }
}
```

Created resources use the same envelope with `S003`, `httpStatus: 201`, and their returned object in `data`. For example, review creation:

```json
{
  "isSuccess": true,
  "responseCode": "S003",
  "message": "Successfully created!",
  "httpStatus": 201,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "bookId": "550e8400-e29b-41d4-a716-446655440000",
    "orderId": "550e8400-e29b-41d4-a716-446655440001",
    "rating": 5,
    "comment": "Excellent read.",
    "imageUrls": [],
    "createdAt": "2026-11-14T08:00:00Z"
  }
}
```

Queued notification and order operations use `202` envelopes. Notification example:

```json
{
  "isSuccess": true,
  "responseCode": "S004",
  "message": "Notification queued for creation.",
  "httpStatus": 202,
  "data": { "notificationId": "550e8400-e29b-41d4-a716-446655440010", "status": "Queued" }
}
```

Queued order example (the exact `data` object depends on the order response DTO):

```json
{
  "isSuccess": true,
  "responseCode": "S005",
  "message": "Order request queued for processing.",
  "httpStatus": 202,
  "data": { "orderId": "550e8400-e29b-41d4-a716-446655440010", "status": "Pending" }
}
```

### Error response examples

Validation errors use the common envelope and include one or more validation messages in `data`:

```json
{
  "isSuccess": false,
  "responseCode": "E001",
  "message": "Request is invalid!",
  "httpStatus": 400,
  "data": ["Email is required", "Invalid email format"]
}
```

Business errors use the same shape. Their `responseCode`, `message` and HTTP status are the values in the error-code reference and endpoint catalog. For example, a missing book:

```json
{
  "isSuccess": false,
  "responseCode": "E005",
  "message": "Data not found!",
  "httpStatus": 404,
  "data": null
}
```

An invalid order state, such as trying to confirm a non-pending order:

```json
{
  "isSuccess": false,
  "responseCode": "E028",
  "message": "Only pending orders can be confirmed.",
  "httpStatus": 409,
  "data": null
}
```

An unhandled argument error includes the exception text in `data`; an unhandled exception has the generic internal-error response:

```json
{
  "isSuccess": false,
  "responseCode": "E002",
  "message": "Internal server error!",
  "httpStatus": 500,
  "data": null
}
```

A rate-limit rejection is not wrapped in `ApiResponse<T>`:

```json
{ "status": 429, "message": "Too many requests. Please try again later." }
```

Authentication and role challenges (`401`/`403`) are generated by ASP.NET Core JWT/authorization middleware, so this application does not define a stable JSON body for them. Depending on the hosting configuration, the body may be empty or a framework-generated response. Image deletion succeeds with `204 No Content` and an empty body. The forecast route returns a raw JSON array, for example:

```json
[
  { "date": "2026-11-15", "temperatureC": 18, "temperatureF": 64, "summary": "Cool" },
  { "date": "2026-11-16", "temperatureC": 23, "temperatureF": 73, "summary": "Mild" }
]
```


## Route-specific success payload examples

The following are illustrative `data` values for the corresponding successful response. Wrap them in the shared success envelope shown above, using the route's success code and HTTP status from the endpoint catalog. Where the controller returns a direct result or no content, that is stated explicitly. Examples are abbreviated only where the DTO contains additional fields described in the endpoint table.

| API | Example success payload (`data`, unless noted) |
|---|---|
| `POST /api/auth/register` | `{"otpExpirySeconds":300}` |
| `POST /api/auth/login` | `{"userId":"550e8400-e29b-41d4-a716-446655440000","accessToken":"eyJ...","refreshToken":"eyJ..."}` |
| `POST /api/auth/verify` | `{"resetPasswordKey":"550e8400-e29b-41d4-a716-446655440000","action":"Register"}` |
| `POST /api/auth/refresh-token` | `{"userId":"550e8400-e29b-41d4-a716-446655440000","accessToken":"eyJ...","refreshToken":"eyJ..."}` |
| `POST /api/auth/logout` | `null` |
| `POST /api/auth/google-sign-in` | `{"userId":"550e8400-e29b-41d4-a716-446655440000","accessToken":"eyJ...","refreshToken":"eyJ..."}` |
| `POST /api/auth/forgot-password` | `{"otpExpirySeconds":300}` |
| `PATCH /api/auth/reset-password` | `null` |
| `POST /api/books/import/new` | `{"totalRows":1,"imported":[{"rowNumber":2,"bookIdentifier":"9780000000001","bookId":"550e8400-e29b-41d4-a716-446655440000","inventoryId":"550e8400-e29b-41d4-a716-446655440001","quantityAdded":3,"totalQuantity":3,"availableQuantity":3}],"failed":[]}` |
| `POST /api/books/import/existing` | `{"totalRows":1,"imported":[{"rowNumber":2,"bookIdentifier":"9780000000001","bookId":"550e8400-e29b-41d4-a716-446655440000","inventoryId":"550e8400-e29b-41d4-a716-446655440001","quantityAdded":3,"totalQuantity":3,"availableQuantity":3}],"failed":[]}` |
| `POST /api/books/inventory/new` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","inventoryId":"550e8400-e29b-41d4-a716-446655440001","quantityAdded":12,"quantity":12,"availableQuantity":12,"bookCreated":true}` |
| `POST /api/books/{bookId}/inventory` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","inventoryId":"550e8400-e29b-41d4-a716-446655440001","quantityAdded":5,"quantity":17,"availableQuantity":17,"bookCreated":false}` |
| `PUT /api/books/{bookId}` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example, Revised"}` |
| `DELETE /api/books/{bookId}` | `null` |
| `PATCH /api/books/{bookId}/restore` | `null` |
| `GET /api/books/{bookId}` | `{"id":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","publisher":"North Press","price":19.99,"status":"Available","rank":1,"createdAt":"2026-11-14T08:00:00Z","category":{"id":"550e8400-e29b-41d4-a716-446655440001","name":"History"},"imageUrls":[],"inventories":[],"averageRating":4.8,"reviewCount":12}` |
| `GET /api/books/{bookId}/promotions` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440010","code":"SAVE10","name":"Ten percent off","type":"PercentageDiscount","discountValue":10,"minimumOrderValue":50,"startAt":"2026-11-14T00:00:00Z","endAt":"2026-12-14T00:00:00Z","usageLimit":100,"usageCount":0,"freeShipping":false}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/books` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","name":"The Example"}],"nextCursor":null}` |
| `GET /api/books/deleted` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","name":"Archived Example"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/books/deleted/search` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","name":"Archived Example"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/books/search` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","name":"The Example"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/category` | `{"id":"550e8400-e29b-41d4-a716-446655440000","name":"History","parentId":null,"isActive":true,"isDefault":false,"createdAt":"2026-11-14T08:00:00Z","updatedAt":null}` |
| `PUT /api/category` | `{"id":"550e8400-e29b-41d4-a716-446655440000","name":"World History","parentId":null,"isActive":true,"isDefault":false,"createdAt":"2026-11-01T08:00:00Z","updatedAt":"2026-11-14T08:00:00Z"}` |
| `DELETE /api/category/{categoryId}` | `null` |
| `PATCH /api/category/{categoryId}/restore` | `null` |
| `GET /api/category/{categoryId}` | `{"id":"550e8400-e29b-41d4-a716-446655440000","name":"History","parentId":null,"isActive":true,"isDefault":false,"createdAt":"2026-11-14T08:00:00Z","updatedAt":null}` |
| `GET /api/category` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","name":"History"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/category/deleted` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","name":"Archived category"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/store` | `{"id":"550e8400-e29b-41d4-a716-446655440000","storeCode":"ST-001","address":"12 Main Street","createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false}` |
| `PUT /api/store` | `{"id":"550e8400-e29b-41d4-a716-446655440000","storeCode":"ST-001","address":"20 Main Street","createdAt":"2026-11-01T08:00:00Z","updatedAt":"2026-11-14T08:00:00Z"}` |
| `DELETE /api/store?Id={guid}` | `null` |
| `PATCH /api/store?Id={guid}` | `null` |
| `GET /api/store` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","storeCode":"ST-001","address":"12 Main Street","createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/store/{Id}` | `{"id":"550e8400-e29b-41d4-a716-446655440000","storeCode":"ST-001","address":"12 Main Street","createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false}` |
| `GET /api/store/deleted` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","storeCode":"ST-001","address":"12 Main Street","isDeleted":true}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/store/search` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440000","storeCode":"ST-001","address":"12 Main Street","createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `GET /api/store/{storeId}/books` | `{"items":[{"bookId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","publisher":"North Press","price":19.99,"status":"Available","quantity":10,"reservedQuantity":2,"availableQuantity":8,"damagedQuantity":0,"incomingQuantity":0,"imageUrls":[]}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/user` | `{"id":"550e8400-e29b-41d4-a716-446655440000","email":"sam@example.com","fullName":"Sam Staff","role":"Employee","phoneNumber":"0123456789","avatarUrl":null,"enabled":true,"createdAt":"2026-11-14T08:00:00Z","storeId":"550e8400-e29b-41d4-a716-446655440001"}` |
| `GET /api/user?Id={guid}` | `{"id":"550e8400-e29b-41d4-a716-446655440000","email":"ari@example.com","fullName":"Ari Reader","role":"Customer","phoneNumber":null,"avatarUrl":null,"enabled":true,"createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false,"storeInfo":null}` |
| `GET /api/user/email` | `{"id":"550e8400-e29b-41d4-a716-446655440000","email":"ari@example.com","fullName":"Ari Reader","role":"Customer","phoneNumber":null,"avatarUrl":null,"enabled":true,"createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false,"storeInfo":null}` |
| `PUT /api/user` | `{"id":"550e8400-e29b-41d4-a716-446655440000","email":"ari@example.com","fullName":"Ari Reader","role":"Customer","phoneNumber":null,"avatarUrl":null,"enabled":true,"createdAt":"2026-11-14T08:00:00Z","updatedAt":null,"isDeleted":false}` |
| `DELETE /api/user?Id={guid}` | `{"listUserIdSuccess":["550e8400-e29b-41d4-a716-446655440000"],"listErrors":[]}` |
| `PATCH /api/user/restore?Id={guid}` | `{"listUserIdSuccess":["550e8400-e29b-41d4-a716-446655440000"],"listErrors":[]}` |
| `DELETE /api/user/bulk` | `{"listUserIdSuccess":["550e8400-e29b-41d4-a716-446655440000"],"listErrors":[]}` |
| `PATCH /api/user/bulk/restore` | `{"listUserIdSuccess":["550e8400-e29b-41d4-a716-446655440000"],"listErrors":[]}` |
| `GET /api/cart/items` | `{"items":[{"bookId":"550e8400-e29b-41d4-a716-446655440000","bookName":"The Example","quantity":2,"unitPrice":19.99}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/cart/items` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","bookName":"The Example","quantity":2,"unitPrice":19.99}` |
| `PUT /api/cart/items/{bookId}` | `{"bookId":"550e8400-e29b-41d4-a716-446655440000","bookName":"The Example","quantity":3,"unitPrice":19.99}` |
| `DELETE /api/cart/items/{bookId}` | `null` |
| `GET /api/orders/summary` | `{"newOrders":4,"inProgressOrders":7,"completedOrders":21,"cancelledOrders":2}` |
| `POST /api/orders` | `{"orderId":"550e8400-e29b-41d4-a716-446655440010","orderCode":"ORD-001","status":"Pending"}` |
| `POST /api/orders/{orderId}/confirm` | `{"orderId":"550e8400-e29b-41d4-a716-446655440010","orderCode":"ORD-001","orderStatus":"Confirmed","wasAlreadyConfirmed":false}` |
| `POST /api/orders/{orderId}/in-transit` | `{"orderId":"550e8400-e29b-41d4-a716-446655440010","orderCode":"ORD-001","orderStatus":"Shipping","wasAlreadyInTransit":false}` |
| `POST /api/orders/{orderId}/complete` | `{"orderId":"550e8400-e29b-41d4-a716-446655440010","orderCode":"ORD-001","orderStatus":"Completed","paymentStatus":"Paid","wasAlreadyCompleted":false}` |
| `POST /api/orders/{orderId}/cancel` | `{"orderId":"550e8400-e29b-41d4-a716-446655440010","orderStatus":"Cancelled","cancelledAt":"2026-11-14T08:00:00Z"}` |
| `GET /api/invoices` | `{"items":[{"invoiceId":"550e8400-e29b-41d4-a716-446655440010","orderCode":"ORD-001","subtotal":39.98,"discountAmount":0,"shippingFee":5,"totalAmount":44.98,"paymentMethod":"CashOnDelivery","paymentStatus":"Pending","orderStatus":"Pending","items":[]}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/orders/{orderId}/returns` | `{"returnRequestId":"550e8400-e29b-41d4-a716-446655440020","orderId":"550e8400-e29b-41d4-a716-446655440010","status":"Pending","requestedAt":"2026-11-14T08:00:00Z"}` |
| `GET /api/returns` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440020","orderId":"550e8400-e29b-41d4-a716-446655440010","status":"Pending"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/returns/{returnRequestId}/confirm` | `{"returnRequestId":"550e8400-e29b-41d4-a716-446655440020","orderId":"550e8400-e29b-41d4-a716-446655440010","status":"Approved","approvedAt":"2026-11-14T08:00:00Z","wasAlreadyApproved":false}` |
| `POST /api/reviews` | `{"id":"550e8400-e29b-41d4-a716-446655440030","bookId":"550e8400-e29b-41d4-a716-446655440000","orderId":"550e8400-e29b-41d4-a716-446655440010","rating":5,"comment":"Excellent read.","imageUrls":[],"createdAt":"2026-11-14T08:00:00Z"}` |
| `GET /api/reviews` | `{"ratingSummary":{"averageRating":4.8,"reviewCount":12,"ratingDistribution":[{"rating":5,"count":10}]},"reviews":{"items":[{"id":"550e8400-e29b-41d4-a716-446655440030","reviewerName":"Ari Reader","rating":5,"comment":"Excellent read.","imageUrls":[],"createdAt":"2026-11-14T08:00:00Z"}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}}` |
| `GET /api/reviews/deleted` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440030","rating":4}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `DELETE /api/reviews/{reviewId}` | `null` |
| `PATCH /api/reviews/{reviewId}/restore` | `null` |
| `POST /api/promotions` | `{"id":"550e8400-e29b-41d4-a716-446655440040","code":"SAVE10","name":"Ten percent off","type":"PercentageDiscount","discountValue":10,"minimumOrderValue":50,"startAt":"2026-11-14T00:00:00Z","endAt":"2026-12-14T00:00:00Z","usageLimit":100,"usageCount":0,"freeShipping":false,"isActive":true,"bookIds":[],"categoryIds":[]}` |
| `POST /api/notifications` | `{"notificationId":"550e8400-e29b-41d4-a716-446655440050","status":"Queued"}` |
| `GET /api/notifications/user/{userId}` | `{"items":[{"id":"550e8400-e29b-41d4-a716-446655440050","userId":"550e8400-e29b-41d4-a716-446655440000","type":"Order","title":"Order confirmed","content":"Your order is confirmed.","channel":"InApp","isRead":false}],"currentPage":1,"pageSize":10,"totalItems":1,"totalPages":1}` |
| `POST /api/image` | Direct JSON response (not `ApiResponse<T>`): `{"objectKey":"books/cover-123.jpg","imageUrl":"https://cdn.example.com/books/cover-123.jpg","contentType":"image/jpeg","sizeBytes":48231}` |
| `DELETE /api/image?objectKey={key}` | `204 No Content`; response body is empty. |
| `GET /api/revenue/by-period` | `{"startDate":"2026-10-01T00:00:00Z","endDate":"2026-11-01T00:00:00Z","period":"month","totalRevenue":1250.50,"orderCount":22,"items":[{"key":"2026-10","name":"October 2026","periodStart":"2026-10-01T00:00:00Z","revenue":1250.50,"orderCount":22,"quantitySold":47}]}` |
| `GET /api/revenue/by-store` | `{"startDate":"2026-10-01T00:00:00Z","endDate":"2026-11-01T00:00:00Z","totalRevenue":1250.50,"orderCount":22,"items":[{"key":"550e8400-e29b-41d4-a716-446655440000","name":"Central","revenue":1250.50,"orderCount":22,"quantitySold":47}]}` |
| `GET /api/revenue/by-product` | `{"totalRevenue":1250.50,"orderCount":22,"items":[{"key":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","revenue":1250.50,"orderCount":22,"quantitySold":47}]}` |
| `GET /api/revenue/by-category` | `{"totalRevenue":1250.50,"orderCount":22,"items":[{"key":"550e8400-e29b-41d4-a716-446655440001","name":"History","revenue":1250.50,"orderCount":22,"quantitySold":47}]}` |
| `GET /api/merchandise/reports/low-stock` | `[{"bookId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","availableQuantity":3,"quantitySold":0}]` |
| `GET /api/merchandise/reports/out-of-stock` | `[{"bookId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","availableQuantity":0,"quantitySold":0}]` |
| `GET /api/merchandise/reports/best-selling` | `[{"bookId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","availableQuantity":null,"quantitySold":47}]` |
| `GET /api/merchandise/reports/slow-moving` | `[{"bookId":"550e8400-e29b-41d4-a716-446655440000","name":"The Example","author":"A. Writer","availableQuantity":8,"quantitySold":2}]` |
| `GET /api/customer-reports/new-customers` | `{"startDate":"2026-10-01T00:00:00Z","endDate":"2026-11-01T00:00:00Z","customerCount":14}` |
| `GET /api/customer-reports/returning-customers` | `{"startDate":"2026-10-01T00:00:00Z","endDate":"2026-11-01T00:00:00Z","customerCount":8}` |
| `GET /api/customer-reports/purchase-frequency` | `{"startDate":"2026-10-01T00:00:00Z","endDate":"2026-11-01T00:00:00Z","totalCompletedOrders":12,"purchasingCustomerCount":8,"averagePurchasesPerCustomer":1.5,"frequencyDistribution":[{"completedOrderCount":1,"customerCount":4},{"completedOrderCount":2,"customerCount":4}]}` |
| `GET /WeatherForecast` | Raw JSON array (not `ApiResponse<T>`): `[{"date":"2026-11-15","temperatureC":18,"temperatureF":64,"summary":"Cool"}]` |

For errors, use the endpoint-specific status and `E` code listed in its endpoint row, the shared error envelope shown in **Error response examples**, and the exact message from the **Error code reference**. Validation responses place their message list in `data`; the exception middleware may place an argument/not-found exception message in `data`. Framework authentication responses, `429`, `204`, and the raw forecast payload retain the distinct formats described above.
