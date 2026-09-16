\# Hướng dẫn set-up



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
  }
}

```
*Lưu ý*: các thông tin về `<host>`, `<port>`, `<database_name>`, `<username>`, `<password>` bạn cần thay thế bằng thông tin thực tế của cơ sở dữ liệu mà bạn đang sử dụng.

**Bước 2***: Thực hiện khởi tạo Migration bằng cách mở Package Manager Console: và chạy lệnh 

```Add-Migration <name>``` 

khi thành công sẽ hiện thông báo ```Build succeeded.```

**Bước 3**:  Thực hiện khởi cập nhật database bằng lệnh 
```Update-Database```
Khi thông công sẽ hiện thông báo cuối cùng là ```done```


