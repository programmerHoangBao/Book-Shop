\# Hướng dẫn set-up



Trong thư mục `back-end`, bạn tạo file `appsettings.json` với nội dung sau:



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
  }
}

```
*Lưu ý*: các thông tin về `<host>`, `<port>`, `<database_name>`, `<username>`, `<password>` bạn cần thay thế bằng thông tin thực tế của cơ sở dữ liệu mà bạn đang sử dụng.


