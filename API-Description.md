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
| `POST /api/user` | Create staff account. JSON `CreateUserRequest`: full name, email, password, role, optional phone/avatar URL and store ID; email/password validation follows registration constraints. | `201 S003`; created user. | `400 E001`; `409 E004`; `404 E018`; `403 E017`; `400 E008`; `500 E002`/`E016`; `401`/`403`; `429`. |
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
