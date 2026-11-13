# Guide to Setting Up and Running the Book Shop Backend in Visual Studio

This guide explains how to set up a local development environment, configure supporting services, update the database with Entity Framework Core, and run the **Book Shop Backend** in **Visual Studio**.

> **Scope:** This guide is for Windows development environments. Example database names, accounts, passwords, service addresses, and AWS Regions are illustrative. Replace them with values for your actual environment.

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Open the project in Visual Studio](#2-open-the-project-in-visual-studio)
3. [Start supporting services](#3-start-supporting-services)
4. [Configure the application](#4-configure-the-application)
5. [Configure AWS S3](#5-configure-aws-s3)
6. [Check and update the database with EF Core](#6-check-and-update-the-database-with-ef-core)
7. [Run the Backend in Visual Studio](#7-run-the-backend-in-visual-studio)
8. [Verify the application after startup](#8-verify-the-application-after-startup)
9. [Troubleshooting](#9-troubleshooting)
10. [Security notes](#10-security-notes)
11. [New-machine setup checklist](#11-new-machine-setup-checklist)
12. [Common commands](#12-common-commands)

---

## 1. Prerequisites

Install and prepare the following components before running the Backend:

| Component | Purpose | Notes |
|---|---|---|
| Visual Studio 2022 | Open, build, debug, and run the Backend | Install the **ASP.NET and web development** workload |
| .NET SDK | Build and run the project | Install an SDK compatible with the `TargetFramework` in `back-end.csproj` |
| PostgreSQL | Store the primary application data | Must be running before migrations or database-dependent features are used |
| Redis | Store temporary data and application state | Check the host, port, and password against the actual configuration |
| Kafka | Send and receive messages/events | Check `BootstrapServers` and listener configuration |
| Entity Framework Core Tools | Create migrations and update the database | The Visual Studio Package Manager Console can be used |

Configure the following when the project or relevant features use them:

- **SMTP email:** send email and OTP codes.
- **Google Authentication:** sign in with Google.
- **Amazon S3:** upload and retrieve images/files.
- **Elasticsearch:** search data according to the application configuration.

### 1.1. Check the .NET SDK

Open PowerShell or a terminal and run:

```powershell
dotnet --info
dotnet --list-sdks
```

Open `back-end.csproj`, check its `TargetFramework`, and make sure a compatible SDK is installed.

### 1.2. Check Visual Studio

In Visual Studio Installer, confirm that this workload is installed:

- **ASP.NET and web development**.

If the solution contains multiple projects, make sure Visual Studio has restored the NuGet packages before continuing.

---

## 2. Open the project in Visual Studio

1. Clone the repository to your computer.
2. Open the `.sln` solution file in Visual Studio. If the repository has no solution file, open `back-end.csproj` directly.
3. In **Solution Explorer**, locate the Backend project (the `back-end` folder/project).
4. Right-click the Backend project and select **Set as Startup Project**.
5. Select the **Debug** configuration.
6. Select **Build → Build Solution** to check for initial compilation errors.

> **Note:** If the solution contains multiple projects, distinguish the **Startup Project** (the project that runs) from the **Default project** in Package Manager Console (the project that receives EF Core commands). In the usual structure of this project, both are `back-end`.

---

## 3. Start supporting services

The Backend must connect to the services configured in the application. Start the relevant services before running the Backend.

### 3.1. PostgreSQL

Make sure PostgreSQL is running and reachable from the Windows machine.

Prepare the following connection details:

- Host: usually `localhost` if PostgreSQL is running locally or Docker has published its port to the host.
- Port: usually `5432`.
- Database name.
- Username and password.

If PostgreSQL runs in Docker, check the containers:

```powershell
docker ps
```

Confirm the container is `Up` and the PostgreSQL port is published correctly.

### 3.2. Redis

Make sure Redis is running and that the connection details in the application configuration match the actual host, port, and password.

If Redis runs in Docker:

```powershell
docker ps
```

### 3.3. Kafka

Make sure Kafka is running before using features that send or receive messages.

If Kafka runs in Docker, check the container and published port. When the Backend runs directly on Windows while Kafka runs in Docker, `BootstrapServers` must use an address reachable from Windows.

> **Kafka note:** Kafka's `advertised.listeners` must be configured for the location where the Backend runs. Publishing a Docker port alone may not be enough if Kafka advertises an internal hostname that Windows cannot resolve.

### 3.4. Elasticsearch and other integrated services

If Elasticsearch search features are enabled, start Elasticsearch and prepare the appropriate URL, username, and password.

SMTP, Google Authentication, and AWS S3 do not necessarily run in Docker. However, their configuration and access permissions must be valid when the related feature is used.

---

## 4. Configure the application

### 4.1. Select the Development configuration file

ASP.NET Core supports environment-specific configuration. For local development, use:

```text
back-end/
├── appsettings.json
├── appsettings.Development.json
└── ...
```

- `appsettings.json`: default configuration, which may contain non-sensitive values.
- `appsettings.Development.json`: configuration specific to the development machine.

If the project already has `appsettings.json`, **do not overwrite it**. Check the existing configuration structure and add any missing settings to `appsettings.Development.json` or User Secrets.

### 4.2. Sample `appsettings.Development.json`

If it does not already exist, create `appsettings.Development.json` in the `back-end` project folder. Use the following structure as a reference:

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
    "DefaultConnection": "Host=localhost;Port=5432;Database=book_shop;Username=postgres;Password=<POSTGRES_PASSWORD>"
  },
  "Security": {
    "SHASecrectKey": "<SHA_SECRET_KEY>",
    "JwtSecretKey": "<JWT_SECRET_KEY>",
    "JwtIssuer": "BookShop-BackEnd",
    "JwtAudience": "BookShop-FrontEnd",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "OtpExpirySeconds": 120,
    "ResetPasswordExpiryMinutes": 5,
    "GlobalRateLimitPermitLimit": 10,
    "LoginRateLimitPermitLimit": 5,
    "OtpRateLimitPermitLimit": 3
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "<SMTP_USERNAME>",
    "Password": "<SMTP_PASSWORD>",
    "FromEmail": "<SENDER_EMAIL>",
    "EnableSsl": true,
    "AppName": "Book Shop"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  },
  "Authentication": {
    "Google": {
      "ClientId": "<GOOGLE_CLIENT_ID>",
      "ApplicationName": "Book-Shop"
    }
  },
  "AWS": {
    "S3": {
      "Region": "<AWS_REGION>",
      "BucketName": "<S3_BUCKET_NAME>",
      "AccessMode": "StableUrl",
      "PresignedUrlExpirationMinutes": 15
    }
  },
  "FileStorage": {
    "MaxFileSizeMB": 5,
    "AllowedContentTypes": [
      "image/jpeg",
      "image/png",
      "image/webp",
      "image/gif"
    ],
    "ImagePrefix": "images"
  },
  "Elasticsearch": {
    "Url": "<ELASTICSEARCH_URL>",
    "Username": "<ELASTICSEARCH_USERNAME>",
    "Password": "<ELASTICSEARCH_PASSWORD>",
    "DefaultIndex": "book_shop"
  }
}
```

**Important:** This is a sample structure based on configuration keys referenced in the project documentation. Values in angle brackets, such as `<...>`, are placeholders, not valid connection details. Replace them with real values before running the application.

### 4.3. Configuration reference

#### PostgreSQL

```text
Host=<postgres_host>;Port=<postgres_port>;Database=<database_name>;Username=<username>;Password=<password>
```

For example, when PostgreSQL runs locally:

```text
Host=localhost;Port=5432;Database=book_shop;Username=postgres;Password=<POSTGRES_PASSWORD>
```

| Property | Meaning |
|---|---|
| `Host` | PostgreSQL address reachable by the Backend |
| `Port` | PostgreSQL port, usually `5432` |
| `Database` | Database name |
| `Username` | PostgreSQL account |
| `Password` | PostgreSQL password |

#### Security

| Key | Meaning |
|---|---|
| `SHASecrectKey` | Secret key used by the application's SHA mechanism |
| `JwtSecretKey` | Secret key used to sign JWTs |
| `JwtIssuer` | JWT issuer |
| `JwtAudience` | JWT audience |
| `AccessTokenExpirationMinutes` | Access token lifetime in minutes |
| `RefreshTokenExpirationDays` | Refresh token lifetime in days |
| `OtpExpirySeconds` | OTP validity period in seconds |
| `ResetPasswordExpiryMinutes` | Reset-password code/token validity period in minutes |
| `GlobalRateLimitPermitLimit` | Request limit under the global rate-limit configuration |
| `LoginRateLimitPermitLimit` | Request limit for the login feature |
| `OtpRateLimitPermitLimit` | Request limit for the OTP feature |

> **Key name note:** The sample retains the spelling `SHASecrectKey` from the source configuration. Do not change it to `SHASecretKey` if the current code reads `SHASecrectKey`; the configuration key must match the code exactly.

Use a strong, randomly generated secret. Avoid easily guessed values such as `123456`, `password`, or `bookshop`.

#### Redis

If Redis does not require a password:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

If Redis requires a password:

```text
localhost:6379,password=<REDIS_PASSWORD>
```

#### SMTP email

Set `Host`, `Port`, `Username`, `Password`, and `FromEmail` according to the SMTP provider used by the project. For Gmail, credentials must meet the security requirements of the Google account. Do not use the regular sign-in password if the provider requires an App Password or another authentication method.

#### Kafka

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

Replace `localhost:9092` with a Kafka address reachable by the Backend process on Windows.

#### Google Authentication

`ClientId` must be a Client ID created and configured for the application in Google Cloud Console. If Google Login is not used in local development, check the service initialization code to determine whether this setting is required at application startup.

#### AWS S3

| Key | Meaning |
|---|---|
| `Region` | AWS Region for the bucket |
| `BucketName` | S3 bucket name |
| `AccessMode` | How the application provides object access paths, for example `StableUrl` or `PresignedUrl` |
| `PresignedUrlExpirationMinutes` | Presigned URL lifetime in minutes |

#### FileStorage

- `MaxFileSizeMB`: maximum file size in the configured units.
- `AllowedContentTypes`: content types permitted for upload.
- `ImagePrefix`: prefix/key prefix used when storing images.

The sample values are for reference. Make sure they match the actual validation and limits in the code.

#### Elasticsearch

- `Url`: Elasticsearch address reachable by the Backend.
- `Username` and `Password`: credentials if Elasticsearch security is enabled.
- `DefaultIndex`: application's default index name.

For a local Elasticsearch instance, the URL is often `http://localhost:9200` or `https://localhost:9200`, depending on the deployment configuration.

### 4.4. Store sensitive values with User Secrets

Do not store passwords, JWT secrets, SHA secrets, SMTP passwords, or Elasticsearch passwords in files committed to Git.

In Visual Studio:

1. Right-click the `back-end` project.
2. Select **Manage User Secrets**.
3. Visual Studio opens the project's `secrets.json` file, which is specific to the current machine.
4. Add sensitive settings using the application's exact configuration key structure.

Example `secrets.json` structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=book_shop;Username=postgres;Password=<POSTGRES_PASSWORD>"
  },
  "Security": {
    "SHASecrectKey": "<SHA_SECRET_KEY>",
    "JwtSecretKey": "<JWT_SECRET_KEY>"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Email": {
    "Username": "<SMTP_USERNAME>",
    "Password": "<SMTP_PASSWORD>"
  },
  "Elasticsearch": {
    "Username": "<ELASTICSEARCH_USERNAME>",
    "Password": "<ELASTICSEARCH_PASSWORD>"
  }
}
```

Non-sensitive values can remain in `appsettings.Development.json`, with passwords and secrets stored separately in User Secrets. Do not add the real contents of `secrets.json` to the repository.

> User Secrets are suitable for local development, not production secret management.

---

## 5. Configure AWS S3

If the Backend uses AWS S3 to upload or retrieve images, configure AWS credentials for the Windows account running Visual Studio.

### 5.1. Check the AWS CLI

Open PowerShell and run:

```powershell
aws --version
```

If the AWS CLI is not installed, install AWS CLI v2 using the official guide:

[Install AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)

### 5.2. Create a Book Shop profile

Run:

```powershell
aws configure --profile book-shop
```

Enter the requested values:

```text
AWS Access Key ID: <YOUR_ACCESS_KEY_ID>
AWS Secret Access Key: <YOUR_SECRET_ACCESS_KEY>
Default region name: <AWS_REGION>
Default output format: json
```

Credentials are stored in the AWS profile for the current Windows user. Do not commit or share the Access Key or Secret Access Key.

### 5.3. Verify credentials

```powershell
aws sts get-caller-identity --profile book-shop
```

If the command returns IAM identity information, the AWS CLI is using the profile you configured.

### 5.4. Configure Visual Studio to use the AWS profile

You can set environment variables for the launch profile used in Visual Studio. Open `Properties/launchSettings.json`, find the profile you use to run the project, and add `AWS_PROFILE` and `AWS_DEFAULT_REGION` to its existing `environmentVariables`.

Example profile configuration:

```json
{
  "profiles": {
    "BookShop": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AWS_PROFILE": "book-shop",
        "AWS_DEFAULT_REGION": "<AWS_REGION>"
      }
    }
  }
}
```

> This is only an example structure. Do not replace the entire `launchSettings.json` with this snippet if the existing file contains `applicationUrl`, other profiles, or other settings. Add the two AWS variables to the profile you use. `AWS_PROFILE` and the Region are not secrets; never put the Access Key/Secret Key directly in this file.

Another option is to set environment variables in PowerShell and start the Backend from that same PowerShell window:

```powershell
$env:AWS_PROFILE = "book-shop"
$env:AWS_DEFAULT_REGION = "<AWS_REGION>"
dotnet run
```

The `$env:` variables apply only to the current PowerShell window and processes started from it. They do not automatically apply to Visual Studio if it was already open.

### 5.5. S3 access permissions

The IAM identity used by the Backend needs the permissions required for the operations performed on the bucket, such as uploading or reading objects.

Configure public read access (`s3:GetObject` through a bucket policy) only when the design intentionally allows anyone to view objects using stable URLs. For private data, keep the bucket private and use presigned URLs (`PresignedUrl`) if that matches the application's implementation.

---

## 6. Check and update the database with EF Core

### 6.1. Open Package Manager Console

In Visual Studio, open:

```text
Tools
→ NuGet Package Manager
→ Package Manager Console
```

In **Package Manager Console**:

1. Select the project containing the `DbContext`/migrations from the **Default project** list. In the usual structure of this project, that is `back-end`.
2. Make sure `back-end` is also selected as the **Startup Project** in Solution Explorer.
3. Wait for NuGet restore and build to finish if Visual Studio is still performing those tasks.

If the solution has separate migration and startup projects, select the correct projects for its actual structure and specify `-Project`/`-StartupProject` when running PMC commands.

### 6.2. Check for existing migrations

Check whether the project has a folder named:

```text
Migrations/
```

If the folder already exists and its migration files are managed in the repository, do not create another `InitialCreate` migration.

You can list migrations from Package Manager Console:

```powershell
Get-Migration
```

### 6.3. If the project already has migrations

If the project has migrations, update the database:

```powershell
Update-Database
```

This applies migrations that have not yet been run to the database specified by `DefaultConnection`.

### 6.4. If the project has no migrations

Only follow this step if the project truly has no migrations and you are initializing the schema for a new database.

Create a migration:

```powershell
Add-Migration InitialCreate
```

You can replace `InitialCreate` with a descriptive change name, for example:

```powershell
Add-Migration CreateBookShopDatabase
```

After the migration is created successfully, the project will have a `Migrations` folder containing the migration files. Then update the database:

```powershell
Update-Database
```

### 6.5. Confirm the database was updated

After `Update-Database` succeeds:

- Open PostgreSQL with your database management tool.
- Check that the database has the tables required by the project design.
- Check that the migration history table, `__EFMigrationsHistory`, was created/updated.

> **Warning:** If the database already contains data or is used by other people, do not delete the database or tables, or recreate migrations, without first checking the schema and migration state.

---

## 7. Run the Backend in Visual Studio

### 7.1. Check the launch profile

Open:

```text
Properties/launchSettings.json
```

Check that the profile you use is configured for the Development environment, for example:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

If you configured an AWS profile in [section 5.4](#54-configure-visual-studio-to-use-the-aws-profile), make sure you select the launch profile that contains both AWS variables.

### 7.2. Build the project

From the Visual Studio menu, select:

```text
Build → Build Solution
```

Proceed to running the application only after the build succeeds. If the build fails, check **Error List** and **Output** for details.

### 7.3. Start the application

In Visual Studio:

- Press **F5** to run with the debugger; or
- Select **Debug → Start Without Debugging** (Ctrl + F5) to run without the debugger.

Select the correct Backend profile before running. Check the **Output** window or console for startup logs and the application URL.

### 7.4. Find the Backend URL

The application address is configured in `Properties/launchSettings.json`, usually through the `applicationUrl` property of the launch profile.

Do not assume a fixed port. Use the actual URL in `launchSettings.json` or the startup log.

---

## 8. Verify the application after startup

After the Backend starts successfully, perform these checks:

1. **Application startup:** no exception stops the host from starting.
2. **Database:** the Backend can connect to PostgreSQL and the database has the required schema.
3. **Redis:** features that need Redis can connect to the Redis server.
4. **Kafka:** the Backend can connect to Kafka when it initializes or uses related features.
5. **S3:** try uploading/retrieving an image if AWS credentials and bucket permissions are configured.
6. **Swagger/OpenAPI:** open the Swagger endpoint according to the project configuration, if Swagger is enabled in Development.

If Swagger does not appear, do not assume that the Backend has failed to start. Check the startup logs, `launchSettings.json`, and Swagger configuration in the code.

---

## 9. Troubleshooting

### 9.1. `Build failed`

If `Add-Migration` or `Update-Database` reports `Build failed`, fix the compilation errors first.

You can check the project from a terminal in the folder containing `back-end.csproj`:

```powershell
dotnet build
```

Review the specific errors in Visual Studio's **Error List** and **Output** windows.

### 9.2. Cannot connect to PostgreSQL

Common errors include:

```text
Connection refused
Failed to connect to PostgreSQL
```

Check the following:

- Is the PostgreSQL service/container running?
- Are `Host` and `Port` correct?
- Does the database exist, or does the account have permission to create it?
- Are `Username` and `Password` correct?
- If PostgreSQL runs in Docker, is its port published to Windows?

### 9.3. Redis connection failed

Check the following:

- The Redis service/container is running.
- `Redis:ConnectionString` has the correct host and port.
- The password is configured correctly if Redis requires authentication.
- Since the Backend runs on Windows, use an address reachable from Windows; do not assume the internal Docker network hostname will work.

### 9.4. Kafka connection failed

Check the following:

- The Kafka broker is running.
- `Kafka:BootstrapServers` has the correct address and port.
- Windows can connect to the published Kafka port.
- `advertised.listeners` does not advertise a hostname reachable only inside the Docker network.

### 9.5. Project or DbContext not found when running migrations

If Package Manager Console cannot find the project or DbContext:

1. Check that **Default project** is set to the project containing the migrations/DbContext.
2. Check that **Startup Project** is set to the correct Backend project.
3. Make sure the project builds successfully.
4. If the solution has multiple projects, explicitly specify `-Project` and `-StartupProject` according to the solution structure.

### 9.6. Database exists but migrations fail

If the database exists but its migration history does not match the source code:

- Run `Get-Migration` to check the migrations in the project.
- Check the `__EFMigrationsHistory` table in the database.
- Determine which migrations were applied and the current schema state.
- Do not delete the database or modify the migration history arbitrarily.

### 9.7. `dotnet ef` is not recognized

If you use the .NET CLI and `dotnet ef` is not recognized, install the tool:

```powershell
dotnet tool install --global dotnet-ef
```

If it is already installed, check it with:

```powershell
dotnet ef --version
```

The `dotnet-ef` version must be compatible with the Entity Framework Core version used by the project.

### 9.8. AWS credentials or S3 access failed

Check in this order:

1. Does `aws sts get-caller-identity --profile book-shop` succeed?
2. Is Visual Studio using the correct launch profile with `AWS_PROFILE=book-shop`?
3. Does `AWS_DEFAULT_REGION` match the bucket's Region?
4. Does the IAM identity have permission to perform the relevant operation on the bucket?
5. Are `AWS:S3:BucketName` and `AWS:S3:Region` correct?

If the AWS CLI command works in PowerShell but the Backend cannot access AWS from Visual Studio, check the launch profile's environment variables. `$env:` variables in a PowerShell window are not automatically applied to Visual Studio if it was launched earlier.

---

## 10. Security notes

Do not commit sensitive information to Git, including:

- PostgreSQL password.
- Redis password.
- JWT and SHA secret keys.
- SMTP username/password or App Password.
- AWS Access Key ID and Secret Access Key.
- Elasticsearch username/password.
- Other credentials or secrets for integrated services.

Recommendations:

1. Use **User Secrets** for secrets in local development.
2. Use placeholders in documentation, such as `<POSTGRES_PASSWORD>`.
3. Check `.gitignore` to avoid committing local configuration files that contain secrets, for example:

```gitignore
**/appsettings.Development.json
**/appsettings.*.Local.json
```

4. Do not put AWS Access Keys/Secret Keys in `appsettings.json`, `appsettings.Development.json`, or `launchSettings.json`.
5. Do not share the contents of `secrets.json`, AWS credentials files, or real secrets in logs/chats.
6. Make an S3 bucket public only when public access is intentional and the risks have been assessed.

If a configuration file was already committed, adding it to `.gitignore` does not automatically remove it from Git history or stop tracking it. Handle its tracking status separately, and rotate any credentials that may have been exposed.

---

## 11. New-machine setup checklist

Use this checklist when a new team member clones the repository:

- [ ] Install Visual Studio and the **ASP.NET and web development** workload.
- [ ] Install a .NET SDK compatible with `back-end.csproj`.
- [ ] Clone the repository and open the solution in Visual Studio.
- [ ] Set `back-end` as the Startup Project.
- [ ] Start PostgreSQL and prepare database credentials.
- [ ] Start Redis and prepare its connection string.
- [ ] Start Kafka and check that its broker address is reachable from Windows.
- [ ] Start Elasticsearch if search features are needed.
- [ ] Configure `appsettings.Development.json` and/or User Secrets.
- [ ] Configure SMTP and Google Authentication if the corresponding features are needed.
- [ ] Configure an AWS CLI profile if S3 upload/retrieval is needed.
- [ ] Restore NuGet packages.
- [ ] Build the solution successfully.
- [ ] Check the `Migrations` folder.
- [ ] Run `Update-Database` if the project already has migrations.
- [ ] Run `Add-Migration` only if the project has no migrations and you are initializing a new schema.
- [ ] Run the Backend with F5 or Ctrl + F5.
- [ ] Check logs, service connections, and Swagger (if enabled).

---

## 12. Common commands

The commands below can be run from a terminal in the folder containing `back-end.csproj`, except for commands specifically marked for Package Manager Console.

### Restore NuGet packages

```powershell
dotnet restore
```

### Build the project

```powershell
dotnet build
```

### List migrations

**Package Manager Console:**

```powershell
Get-Migration
```

**.NET CLI:**

```powershell
dotnet ef migrations list
```

### Create a migration

Run this only when a new migration is needed after changing the model/schema, and after confirming that a corresponding migration does not already exist.

**Package Manager Console:**

```powershell
Add-Migration <MigrationName>
```

**.NET CLI:**

```powershell
dotnet ef migrations add <MigrationName>
```

### Update the database

**Package Manager Console:**

```powershell
Update-Database
```

**.NET CLI:**

```powershell
dotnet ef database update
```

### Run the Backend with the .NET CLI

```powershell
dotnet run
```

### Check the AWS profile

```powershell
aws sts get-caller-identity --profile book-shop
```

---

## Overall workflow

```text
Clone repository
       ↓
Install Visual Studio + compatible .NET SDK
       ↓
Open the solution and set back-end as the Startup Project
       ↓
Start PostgreSQL / Redis / Kafka
       ↓
Start Elasticsearch if needed
       ↓
Create appsettings.Development.json and configure User Secrets
       ↓
Configure an AWS profile if using Amazon S3
       ↓
Restore NuGet packages
       ↓
Build the solution
       ↓
Check the Migrations folder
       ↓
Migrations exist? ── Yes ──→ Update-Database
       │
       └── No ─────────────→ Add-Migration InitialCreate
                                   ↓
                             Update-Database
                                   ↓
                         Run Backend with F5/Ctrl + F5
                                   ↓
                         Check logs and API/Swagger
```

> **Remember:** If the repository already contains a `Migrations` folder, you usually only need to configure the database connection and run `Update-Database`. Do not create `InitialCreate` again in a project that already has migrations.
