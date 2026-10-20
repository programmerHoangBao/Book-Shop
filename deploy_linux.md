# Hướng dẫn triển khai dự án trên server linux

# Table of Contents

- [Hướng dẫn triển khai dự án trên server linux](#hướng-dẫn-triển-khai-dự-án-trên-server-linux)
- [Table of Contents](#table-of-contents)
  - [1. Triển khai trên server linux](#1-triển-khai-trên-server-linux)
    - [Bước 1: Cài đặt Open-SSH trên server linux](#bước-1-cài-đặt-open-ssh-trên-server-linux)
    - [Bước 2: Kết nối server linux thông qua SSH](#bước-2-kết-nối-server-linux-thông-qua-ssh)
    - [Bước 3: Cài đặt Docker Engine và Docker Compose.](#bước-3-cài-đặt-docker-engine-và-docker-compose)
    - [Bước 4: Clone dự án Book-shop từ github](#bước-4-clone-dự-án-book-shop-từ-github)
    - [Bước 5: Triển khai dự án trên docker](#bước-5-triển-khai-dự-án-trên-docker)
    - [Bước 6: Thực hiện Migration database](#bước-6-thực-hiện-migration-database)

## 1. Triển khai trên server linux

### Bước 1: Cài đặt Open-SSH trên server linux

- Trên server linux thực hiện lệnh:
  ```base
  $ sudo apt update
  $ sudo apt install -y openssh-server
  $ sudo systemctl enable --now ssh
  $ sudo systemctl status ssh
  ```
- Nếu đúng thì bạn sẽ thấy log in ra của lệnh `sudo systemctl status ssh` sẽ có thông tin `Active` là `active (running)`
  <img width="755" height="590" alt="image" src="https://github.com/user-attachments/assets/df06ddc7-ccdb-47fe-9083-c1e4d4e078dd" />

### Bước 2: Kết nối server linux thông qua SSH

- Trong server linux kiểm tra xem ip mạng của server linux bằng lệnh

  ```base
  $ ip addr
  ```

 <img width="678" height="209" alt="Screenshot 2026-10-17 230000" src="https://github.com/user-attachments/assets/2883dbc2-9f7a-420b-8326-0fd7d86b117a" />

- Trên thiếp bị các nhân của bạn mở terminal thực hiện lệnh:
  ```base
  $ ssh <USERNAME>@<IP_SERVER_LINUX>
  ```
- Trong đó `USERNAME` và <IP_SERVER_LINUX> là tên đăng nhập và ip mạng của server linux ví dụ: `ssh bao@192.168.112.178`

<img width="464" height="479" alt="image" src="https://github.com/user-attachments/assets/ae3edf5d-1995-4db1-80f8-c3aecdb43950" />

### Bước 3: Cài đặt Docker Engine và Docker Compose.

- Toàn bộ lệnh trong bước này được chạy trên Ubuntu Server qua phiên SSH.
- Docker Engine sẽ chạy các container; Docker Compose plugin cho phép bạn quản lý nhiều service trong cùng một file YAML
- Cập nhật hệ thống:

  ```base
  $ sudo apt upgrade -y
  $ sudo apt install -y ca-certificates curl git
  ```

- Thêm repository chính thức của docker:

  ```base
  $ sudo install -m 0755 -d /etc/apt/keyrings
  $ sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
  -o /etc/apt/keyrings/docker.asc
  $ sudo chmod a+r /etc/apt/keyrings/docker.asc
  $ sudo tee /etc/apt/sources.list.d/docker.sources <<EOF
  Types: deb
  URIs: https://download.docker.com/linux/ubuntu
  Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
  Components: stable
  Architectures: $(dpkg --print-architecture)
  Signed-By: /etc/apt/keyrings/docker.asc
  EOF
  ```

  <img width="695" height="380" alt="image" src="https://github.com/user-attachments/assets/0c1a8edb-4dbe-4484-9d55-0fd6f02296f6" />

- Cài Docker Engine và Compose:

  ```base
  $ sudo apt update
  $ sudo apt install -y \
  docker-ce \
  docker-ce-cli \
  containerd.io \
  docker-buildx-plugin \
  docker-compose-plugin
  ```

  <img width="1181" height="749" alt="image" src="https://github.com/user-attachments/assets/fd7e9aa4-b414-4a54-ab3b-7f62cc43edea" />

- Kiểm tra docker:

  ```base
  $ sudo systemctl enable --now docker
  $ sudo docker run hello-world
  $ sudo docker --version
  $ sudo docker compose version
  ```

  <img width="958" height="734" alt="image" src="https://github.com/user-attachments/assets/b3ea60f3-7405-40dd-91ac-41fb3f3164ca" />

### Bước 4: Clone dự án Book-shop từ github

- Tạo thư mục triển khai:

  ```base
  $ mkdir -p ~/projects
  $ cd ~/projects
  ```

- Clone dự án:

  ```base
  $ git clone https://github.com/programmerHoangBao/Book-Shop.git
  $ cd Book-Shop
  ```

  - Tạo các file .env, back-end/appsettings.Production.json.
  - File .env có cấu trúc như sau:

    ```
    # PostgreSQL
    POSTGRES_DB=............
    POSTGRES_USER=...........
    POSTGRES_PASSWORD=...................

    # Redis
    REDIS_PASSWORD=......................
    ```

  - File back-end/appsettings.json có nội dung như sau:
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
        "DefaultConnection": "Server=localhost;Port=5432;Database=<database-name>;Username=<username-postgresql>;Password=<password-postgresql>"
      },
      "Security": {
        "SHASecrectKey": "<sha-secrect-key>",
        "JwtSecretKey": "<jwt-secrect-key>",
        "JwtIssuer": "BookShop-BackEnd",
        "JwtAudience": "BookShop-FrontEnd",
        "AccessTokenExpirationMinutes": <Access-Token-Expiration-Minutes>,
        "RefreshTokenExpirationDays": <Refresh-Token-Expiration-Days>,
        "OtpExpirySeconds": <Otp-Expiry-Seconds>,
        "ResetPasswordExpiryMinutes": <Reset-Password-Expiry-Minutes>,
        "GlobalRateLimitPermitLimit": 10,
        "LoginRateLimitPermitLimit": 5,
        "OtpRateLimitPermitLimit": 3
      },
      "Redis": {
        "ConnectionString": "localhost:6379,password=<redis-password>"
      },
      "Email": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "Username": "<username-app-email>",
        "Password": "<password-app-email>",
        "FromEmail": "<from-email>",
        "EnableSsl": true,
        "AppName": "Book Shop"
      },
      "Kafka": {
        "BootstrapServers": "localhost:29092"
      },
      "Authentication": {
        "Google": {
          "ClientId": "<google-client-id>"
        }
      }
    }
    ```
  - File back-end/appsettings.Production.json có nội dung như sau:
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
        "DefaultConnection": "Server=postgresql;Port=5432;Database=<database-name>;Username=<username-postgresql>;Password=<password-postgresql>"
      },
      "Security": {
        "SHASecrectKey": "<sha-secrect-key>",
        "JwtSecretKey": "<jwt-secrect-key>",
        "JwtIssuer": "BookShop-BackEnd",
        "JwtAudience": "BookShop-FrontEnd",
        "AccessTokenExpirationMinutes": <Access-Token-Expiration-Minutes>,
        "RefreshTokenExpirationDays": <Refresh-Token-Expiration-Days>,
        "OtpExpirySeconds": <Otp-Expiry-Seconds>,
        "ResetPasswordExpiryMinutes": <Reset-Password-Expiry-Minutes>,
        "GlobalRateLimitPermitLimit": 10,
        "LoginRateLimitPermitLimit": 5,
        "OtpRateLimitPermitLimit": 3
      },
      "Redis": {
        "ConnectionString": "redis:6379,password=<redis-password>"
      },
      "Email": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "Username": "<username-app-email>",
        "Password": "<password-app-email>",
        "FromEmail": "<from-email>",
        "EnableSsl": true,
        "AppName": "Book Shop"
      },
      "Kafka": {
        "BootstrapServers": "kafka:9092"
      },
      "Authentication": {
        "Google": {
          "ClientId": "<google-client-id>"
        }
      }
    }
    ```
  - Cập nhật các giá trị của các `<database-name>`, `<username-postgresql>`, `<password-postgresql>`, `<sha-secrect-key>`, `<jwt-secrect-key>`, `<Access-Token-Expiration-Minutes>`, `<Refresh-Token-Expiration-Days>`, `<Otp-Expiry-Seconds>`, `<Reset-Password-Expiry-Minutes>`, `<redis-password>`, `<username-app-email>`, `<password-app-email>`, `<from-email>`, `<google-client-id>` cho phù hợp

Cập nhật các giá trị được đặt trong dấu `<...>` theo thông tin cấu hình thực tế của môi trường triển khai.

| Placeholder                         | Mô tả                                                                                                                                                                                                                                |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `<database-name>`                   | Tên cơ sở dữ liệu PostgreSQL được sử dụng để lưu trữ dữ liệu của hệ thống Book Shop. Giá trị phải trùng với tên database đã được tạo trong PostgreSQL.                                                                               |
| `<username-postgresql>`             | Tên tài khoản PostgreSQL được cấp quyền truy cập vào database của ứng dụng.                                                                                                                                                          |
| `<password-postgresql>`             | Mật khẩu của tài khoản PostgreSQL tương ứng với `<username-postgresql>`.                                                                                                                                                             |
| `<sha-secrect-key>`                 | Khóa bí mật được sử dụng trong các chức năng băm dữ liệu bằng SHA của hệ thống. Cần sử dụng một chuỗi bí mật đủ mạnh, có độ ngẫu nhiên cao và không chia sẻ công khai.                                                               |
| `<jwt-secrect-key>`                 | Khóa bí mật được sử dụng để ký và xác thực JSON Web Token (JWT). Khóa cần đủ dài và mạnh theo thuật toán ký JWT đang được sử dụng, đồng thời phải được bảo mật và giữ ổn định trong quá trình triển khai.                            |
| `<Access-Token-Expiration-Minutes>` | Thời gian hiệu lực của Access Token, tính bằng phút. Ví dụ: `15` tương ứng với 15 phút.                                                                                                                                              |
| `<Refresh-Token-Expiration-Days>`   | Thời gian hiệu lực của Refresh Token, tính bằng ngày. Ví dụ: `7` tương ứng với 7 ngày.                                                                                                                                               |
| `<Otp-Expiry-Seconds>`              | Thời gian hiệu lực của mã OTP, tính bằng giây. Ví dụ: `300` tương ứng với 5 phút.                                                                                                                                                    |
| `<Reset-Password-Expiry-Minutes>`   | Thời gian hiệu lực của khóa hoặc mã dùng để xác thực yêu cầu đặt lại mật khẩu, tính bằng phút. Ví dụ: `10` tương ứng với 10 phút.                                                                                                    |
| `<redis-password>`                  | Mật khẩu xác thực của Redis Server được cấu hình trong môi trường triển khai. Giá trị phải khớp với mật khẩu Redis thực tế.                                                                                                          |
| `<username-app-email>`              | Tên tài khoản email được sử dụng để đăng nhập máy chủ SMTP và gửi email tự động từ hệ thống. Với Gmail, thông thường đây là địa chỉ Gmail đầy đủ.                                                                                    |
| `<password-app-email>`              | Mật khẩu ứng dụng (App Password) dùng để xác thực SMTP của Gmail. Nên sử dụng App Password thay vì mật khẩu đăng nhập Gmail thông thường khi tài khoản đáp ứng các điều kiện cần thiết.                                              |
| `<from-email>`                      | Địa chỉ email người gửi hiển thị trên các email do hệ thống gửi đi, chẳng hạn email chứa mã OTP xác thực hoặc đặt lại mật khẩu. Thông thường, địa chỉ này trùng với tài khoản được cấu hình tại `<username-app-email>`.              |
| `<google-client-id>`                | Client ID của OAuth 2.0 được cấp cho ứng dụng Book Shop trong Google Cloud Console, dùng để cấu hình và xác thực chức năng đăng nhập bằng Google. Cần sử dụng Client ID phù hợp với loại ứng dụng và luồng xác thực đang triển khai. |

**Lưu ý khi cấu hình:**

- Thay thế toàn bộ placeholder bằng giá trị thực tế trước khi chạy ứng dụng. Các placeholder như `<Access-Token-Expiration-Minutes>` phải được thay bằng giá trị số hợp lệ, không giữ nguyên dấu `<` và `>`.
- Các giá trị thời hạn phải là số nguyên dương và phù hợp với kiểu dữ liệu mà ứng dụng sử dụng.
- Đối với Gmail SMTP, cần bật xác minh hai bước và tạo App Password nếu tài khoản hỗ trợ tính năng này.
- Không sử dụng mật khẩu, khóa bí mật hoặc thông tin xác thực thật trong mã nguồn công khai hoặc Git repository. Nên sử dụng biến môi trường, Docker Secrets hoặc một hệ thống quản lý bí mật phù hợp cho môi trường Production.
- Các tên host như `postgresql`, `redis` và `kafka` phải khớp với tên service hoặc hostname có thể truy cập từ container backend trong mạng Docker.
- Sau khi cập nhật cấu hình, cần kiểm tra kết nối PostgreSQL, Redis, Kafka, SMTP và chức năng đăng nhập Google để bảo đảm hệ thống hoạt động đúng.

### Bước 5: Triển khai dự án trên docker

- Bạn có thể kiểm tra của docker của dự án, các services, volumns sẽ triển khai:

  ```base
  $ sudo docker compose config -q
  $ sudo docker compose config --services
  $ sudo docker compose config --volumes
  ```

  <img width="608" height="219" alt="image" src="https://github.com/user-attachments/assets/acf87863-7623-44af-89ca-25fadd920a94" />

- Tải các image cần thiết từ registry cho các service được định nghĩa trong file `docker-compose.yml`:

  ```base
  $ sudo docker compose pull
  ```

  <img width="571" height="123" alt="image" src="https://github.com/user-attachments/assets/4d355a59-5a89-4056-8a38-38074dfe266b" />

- Build lại toàn bộ image cho các service trong file docker-compose.yml mà không sử dụng cache:

  ```base
  $ sudo docker compose build --no-cache
  ```

  Khi thành công sẽ có thông báo `✔ Image book-shop-back-end Built`:

  <img width="574" height="81" alt="image" src="https://github.com/user-attachments/assets/17541c71-e7f6-4f68-bb72-059a78500569" />

- Khởi động các container được định nghĩa trong file `docker-compose.yml`:

  ```base
  $ sudo docker compose up -d
  ```

  <img width="523" height="199" alt="image" src="https://github.com/user-attachments/assets/6d1013ba-b048-4dff-bafc-a6e945560ef2" />

- Kiểm tra xem các container đã được triển khai thành công hay chưa bằng lệnh:

  ```base
  $ sudo docker compose ps
  ```

  Kết quả thu được là:

  <img width="1271" height="215" alt="image" src="https://github.com/user-attachments/assets/42c6bc06-e191-45de-9c6a-e0957092882b" />

### Bước 6: Thực hiện Migration database

- Cài đạt dotnet 9.0 trên linux:

  ```base
  $ cat /etc/os-release
  $ apt-cache policy dotnet-host dotnet-host-9.0 dotnet-hostfxr-9.0 dotnet-sdk-9.0
  sudo apt update
  $ sudo apt install -y software-properties-common
  $ sudo add-apt-repository ppa:dotnet/backports
  $ sudo apt update
  $ sudo apt install -y dotnet-sdk-9.0
  $ dotnet --info
  ```

  Kết quả đạt được là:

  <img width="440" height="278" alt="image" src="https://github.com/user-attachments/assets/d8b30ae9-659a-4228-9051-e201bbbc762b" />

- Cài đặt Entity Framework Core Command-Line Tools (dotnet-ef) vào máy tính của bạn dưới dạng một công cụ toàn cục (global tool) bằng lệnh:

```base
$ dotnet tool install --global dotnet-ef --version 9.*
$ echo 'export PATH="$HOME/.dotnet/tools:$PATH"' >> ~/.bashrc
$ source ~/.bashrc
$ dotnet ef --version
```

<img width="814" height="140" alt="image" src="https://github.com/user-attachments/assets/05072a23-8ca2-4c29-9d66-302cd18bf565" />

- Thực hiện lệnh cập nhật database theo Miragation:

```base
$ cd back-end
$ dotnet ef database update
```

Kết quả nhận được `Build succeeded.` và `done` nghĩa là việc Miragtion đã thành công.

<img width="1227" height="404" alt="Screenshot 2026-10-18 014251" src="https://github.com/user-attachments/assets/2a0ef7f1-8b04-4580-b504-fcaff84e708a" />
