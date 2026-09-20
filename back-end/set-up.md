# Hướng dẫn set-up

**Bước 1**: Trong thư mục `back-end`, bạn tạo file `appsettings.json` với nội dung sau:

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
    "DefaultConnection": "Server=<host>;Port=<port>;Database=<database_name>;Username=<username>;Password=<password>"
  },
  "Security": {
    "SHASecrectKey": "<sha_secrect_key>",
    "JwtSecretKey": "<jwt_secret_key>",
    "JwtIssuer": "BookShop-BackEnd",
    "JwtAudience": "BookShop-FrontEnd",
    "AccessTokenExpirationMinutes": <int>,
    "RefreshTokenExpirationDays": <int>,
    "OtpExpirySeconds": <int>
  },
  "Redis": {
    "ConnectionString": "<host>:<port>,password=<password>"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "<email>",
    "Password": "<password>",
    "FromEmail": "<email>",
    "EnableSsl": true,
    "AppName": "Book Shop"
  },
  "Kafka": {
    "BootstrapServers": "<host>:<port>"
  }
}
```

**Bước 2***: Thực hiện khởi tạo Migration bằng cách mở Package Manager Console: và chạy lệnh 

```Add-Migration <name>``` 

khi thành công sẽ hiện thông báo ```Build succeeded.```

**Bước 3**:  Thực hiện khởi cập nhật database bằng lệnh 
```Update-Database```
Khi thông công sẽ hiện thông báo cuối cùng là ```done```


