# Hướng dẫn cài đặt Backend Book Shop

Tài liệu này hướng dẫn cách cấu hình môi trường, kết nối cơ sở dữ liệu và thực hiện Entity Framework Core Migration để chạy Backend của dự án **Book Shop**.

## 1. Yêu cầu môi trường

Trước khi chạy Backend, cần cài đặt các thành phần sau:

* .NET SDK phù hợp với phiên bản của project.
* PostgreSQL.
* Redis.
* Kafka.
* Visual Studio 2022 hoặc IDE tương đương.
* Entity Framework Core Tools nếu sử dụng Package Manager Console hoặc `dotnet ef`.

Đảm bảo các service cần thiết đã được khởi động trước khi chạy Backend.

> **Lưu ý:** Tùy vào cấu hình thực tế của project, Redis hoặc Kafka có thể được chạy bằng Docker. Hãy đảm bảo `host` và `port` trong `appsettings.json` khớp với môi trường đang sử dụng.

---

# 2. Cấu hình `appsettings.json`

Trong thư mục `back-end`, tạo file:

```text
appsettings.json
```

Cấu trúc cấu hình tham khảo:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "AllowedHosts": "*",

  "ConnectionStrings": {
    "DefaultConnection": "Host=<postgres_host>;Port=<postgres_port>;Database=<database_name>;Username=<username>;Password=<password>"
  },

  "Security": {
    "SHASecrectKey": "<sha_secret_key>",
    "JwtSecretKey": "<jwt_secret_key>",
    "JwtIssuer": "<jwt_issuer>",
    "JwtAudience": "<jwt_audience>",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7,
    "OtpExpirySeconds": 60,
    "ResetPasswordExpiryMinutes": 15
  },

  "Redis": {
    "ConnectionString": "<redis_host>:<redis_port>,password=<redis_password>"
  },

  "Email": {
    "Host": "<smtp_host>",
    "Port": 587,
    "Username": "<email_username>",
    "Password": "<email_password>",
    "FromEmail": "<sender_email>",
    "EnableSsl": true,
    "AppName": "Book Shop"
  },

  "Kafka": {
    "BootstrapServers": "<kafka_host>:<kafka_port>"
  },

  "Authentication": {
    "Google": {
      "ClientId": "<google_client_id>"
    }
  }
}
```

## 2.1. Giải thích các cấu hình

### PostgreSQL

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=<postgres_host>;Port=<postgres_port>;Database=<database_name>;Username=<username>;Password=<password>"
}
```

Ví dụ khi PostgreSQL chạy local:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=book_shop;Username=postgres;Password=123456"
}
```

Các giá trị cần thay đổi:

| Giá trị           | Ý nghĩa                           |
| ----------------- | --------------------------------- |
| `<postgres_host>` | Địa chỉ PostgreSQL                |
| `<postgres_port>` | Port PostgreSQL, thường là `5432` |
| `<database_name>` | Tên database                      |
| `<username>`      | Username PostgreSQL               |
| `<password>`      | Password PostgreSQL               |

---

### Security

```json
"Security": {
  "SHASecrectKey": "<sha_secret_key>",
  "JwtSecretKey": "<jwt_secret_key>",
  "JwtIssuer": "<jwt_issuer>",
  "JwtAudience": "<jwt_audience>",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7,
  "OtpExpirySeconds": 60,
  "ResetPasswordExpiryMinutes": 15
}
```

Trong đó:

* `SHASecrectKey`: Secret key sử dụng cho cơ chế SHA của ứng dụng.
* `JwtSecretKey`: Secret key dùng để ký JWT.
* `JwtIssuer`: Issuer của JWT.
* `JwtAudience`: Audience của JWT.
* `AccessTokenExpirationMinutes`: thời gian hết hạn Access Token.
* `RefreshTokenExpirationDays`: thời gian hết hạn Refresh Token.
* `OtpExpirySeconds`: thời gian OTP có hiệu lực.
* `ResetPasswordExpiryMinutes`: thời gian hiệu lực của mã/token reset password.

Nên sử dụng secret key ngẫu nhiên, đủ dài và không sử dụng các chuỗi dễ đoán như:

```text
123456
password
secret
bookshop
```

---

### Redis

```json
"Redis": {
  "ConnectionString": "<redis_host>:<redis_port>,password=<redis_password>"
}
```

Ví dụ Redis chạy local:

```json
"Redis": {
  "ConnectionString": "localhost:6379,password=123456"
}
```

Nếu Redis không yêu cầu password thì có thể cấu hình:

```json
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

---

### Email SMTP

```json
"Email": {
  "Host": "<smtp_host>",
  "Port": 587,
  "Username": "<email_username>",
  "Password": "<email_password>",
  "FromEmail": "<sender_email>",
  "EnableSsl": true,
  "AppName": "Book Shop"
}
```

Các thông tin này phụ thuộc vào nhà cung cấp email/SMTP mà project sử dụng.

Không nên commit username/password email lên Git repository công khai.

---

### Kafka

```json
"Kafka": {
  "BootstrapServers": "<kafka_host>:<kafka_port>"
}
```

Ví dụ Kafka chạy local:

```json
"Kafka": {
  "BootstrapServers": "localhost:9092"
}
```

Đảm bảo Kafka đang chạy trước khi Backend cần sử dụng các chức năng liên quan đến message/event.

---

### Google Authentication

```json
"Authentication": {
  "Google": {
    "ClientId": "<google_client_id>"
  }
}
```

Thay `<google_client_id>` bằng Client ID được tạo từ Google Cloud Console.

---

# 3. Kiểm tra file cấu hình

Sau khi tạo `appsettings.json`, kiểm tra lại:

* PostgreSQL đã chạy.
* Database server có thể kết nối.
* Redis đã chạy.
* Kafka đã chạy.
* SMTP/email configuration chính xác.
* Google Client ID hợp lệ nếu sử dụng Google Login.
* Các secret key đã được cấu hình.

Đặc biệt kiểm tra `Host`, `Port`, `Username`, `Password` của PostgreSQL.

---

# 4. Khởi tạo Entity Framework Core Migration

## 4.1. Kiểm tra Migration hiện có

Trong project, kiểm tra xem đã tồn tại thư mục:

```text
Migrations
```

hay chưa.

### Trường hợp 1: Project chưa có Migration

Mở:

```text
Visual Studio
→ Tools
→ NuGet Package Manager
→ Package Manager Console
```

Sau đó chạy:

```powershell
Add-Migration InitialCreate
```

Có thể thay `InitialCreate` bằng tên migration phù hợp, ví dụ:

```powershell
Add-Migration CreateBookShopDatabase
```

Khi build thành công, Visual Studio thường hiển thị:

```text
Build succeeded.
```

Sau đó project sẽ tạo thư mục `Migrations` chứa các file migration.

---

## 4.2. Trường hợp project đã có Migration

Nếu repository đã chứa thư mục `Migrations` và các migration đã được tạo sẵn thì **không cần chạy `Add-Migration` lại**.

Chỉ cần thực hiện:

```powershell
Update-Database
```

Việc tạo migration mới chỉ cần thiết khi model/entity của project có thay đổi và cần cập nhật schema database.

---

# 5. Cập nhật Database

Sau khi migration đã được tạo, chạy:

```powershell
Update-Database
```

Lệnh này sẽ áp dụng các migration chưa được thực thi vào PostgreSQL database.

Nếu thực hiện thành công, Package Manager Console sẽ hiển thị thông tin migration đã được áp dụng và kết thúc mà không có lỗi.

Có thể kiểm tra trực tiếp trong PostgreSQL để xác nhận database đã được tạo đầy đủ các bảng.

---

# 6. Sử dụng `dotnet ef` thay cho Package Manager Console

Nếu không sử dụng Visual Studio Package Manager Console, có thể thực hiện bằng .NET CLI.

Tạo migration:

```bash
dotnet ef migrations add InitialCreate
```

Cập nhật database:

```bash
dotnet ef database update
```

Nếu máy chưa nhận lệnh `dotnet ef`, có thể cài Entity Framework Core CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

Sau khi cài đặt, kiểm tra:

```bash
dotnet ef --version
```

---

# 7. Xử lý lỗi thường gặp

## 7.1. `Build failed`

Khi chạy:

```powershell
Add-Migration InitialCreate
```

nếu xuất hiện:

```text
Build failed.
```

thì cần kiểm tra lỗi build của project trước.

Có thể chạy:

```bash
dotnet build
```

để xem chi tiết lỗi.

Migration không thể được tạo nếu project đang có lỗi compile.

---

## 7.2. Không kết nối được PostgreSQL

Nếu gặp các lỗi như:

```text
Connection refused
```

hoặc:

```text
Failed to connect to PostgreSQL
```

hãy kiểm tra:

```text
Host
Port
Database
Username
Password
```

Trong trường hợp PostgreSQL chạy bằng Docker, cần kiểm tra container:

```bash
docker ps
```

và xác nhận PostgreSQL đang ở trạng thái `Up`.

---

## 7.3. Redis connection failed

Kiểm tra Redis đã chạy hay chưa:

```bash
docker ps
```

hoặc kiểm tra trực tiếp service Redis.

Đồng thời kiểm tra:

```json
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

có đúng với môi trường thực tế hay không.

---

## 7.4. Kafka connection failed

Kiểm tra:

```json
"Kafka": {
  "BootstrapServers": "localhost:9092"
}
```

và đảm bảo Kafka đang chạy đúng port.

Nếu Kafka chạy trong Docker nhưng Backend chạy trên máy host, cần đặc biệt kiểm tra cấu hình advertised listeners của Kafka để Backend có thể kết nối được.

---

## 7.5. Database đã tồn tại nhưng Migration bị lỗi

Nếu database đã tồn tại và migration history không đồng bộ với project, không nên tùy tiện xóa database.

Trước tiên kiểm tra các migration hiện có:

```powershell
Get-Migration
```

hoặc:

```bash
dotnet ef migrations list
```

Sau đó xác định migration nào đã được áp dụng trước khi thực hiện thêm thay đổi.

---

# 8. Bảo mật thông tin cấu hình

**Không nên commit các thông tin nhạy cảm vào Git**, đặc biệt:

```text
Database password
Redis password
JWT secret key
SHA secret key
Email password
Google Client ID/Secret
```

Đối với môi trường local, có thể sử dụng:

```text
appsettings.Development.json
```

hoặc ASP.NET Core User Secrets để lưu các thông tin nhạy cảm.

Ví dụ không nên đưa trực tiếp vào repository:

```json
"Password": "my-real-password"
```

Thay vào đó, README chỉ nên chứa placeholder:

```json
"Password": "<password>"
```

Ngoài ra, nên kiểm tra `.gitignore` để tránh vô tình commit các file cấu hình chứa secret.

---

# 9. Chạy Backend

Sau khi:

1. Cấu hình `appsettings.json`.
2. Khởi động PostgreSQL.
3. Khởi động Redis.
4. Khởi động Kafka.
5. Tạo hoặc kiểm tra Migration.
6. Chạy `Update-Database`.

Có thể chạy project bằng Visual Studio:

```text
Debug → Start Without Debugging
```

hoặc:

```bash
dotnet run
```

URL chạy ứng dụng được cấu hình trong:

```text
Properties/launchSettings.json
```

Sau khi Backend khởi động thành công, kiểm tra Swagger nếu project đã tích hợp Swagger/OpenAPI.

---

# 10. Quy trình cài đặt tóm tắt

Đối với một máy mới clone project về, quy trình cơ bản:

```text
Clone project
      ↓
Cài .NET SDK
      ↓
Khởi động PostgreSQL / Redis / Kafka
      ↓
Tạo appsettings.json
      ↓
Điền Connection String + Security + Redis + Email + Kafka + Google
      ↓
dotnet restore
      ↓
dotnet build
      ↓
Kiểm tra Migration
      ↓
Add-Migration InitialCreate   (chỉ khi chưa có Migration)
      ↓
Update-Database
      ↓
dotnet run
      ↓
Kiểm tra API / Swagger
```

## 11. Các lệnh thường sử dụng

### Restore package

```bash
dotnet restore
```

### Build project

```bash
dotnet build
```

### Tạo Migration

```bash
dotnet ef migrations add InitialCreate
```

### Xem danh sách Migration

```bash
dotnet ef migrations list
```

### Cập nhật Database

```bash
dotnet ef database update
```

### Chạy Backend

```bash
dotnet run
```

---

## 12. Lưu ý quan trọng

Nếu project đã được cung cấp sẵn thư mục `Migrations`, **không tạo lại `InitialCreate`**. Chỉ cần cấu hình database và chạy:

```bash
dotnet ef database update
```

Nếu project chưa có Migration thì thực hiện:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Sau khi cấu hình xong, nếu Backend khởi động không thành công, nên kiểm tra theo thứ tự:

```text
Configuration
→ Database
→ Redis
→ Kafka
→ Email
→ Build
→ Application startup
```
