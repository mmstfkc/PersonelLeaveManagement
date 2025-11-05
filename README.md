# Kurulum

dotnet kurulumu kontrol et
```bash
dotnet --version
```


Src klasörünü oluştur ve içine gir
```bash
mkdir src
cd src
```

## Solution Oluştur

```bash
dotnet new sln -n PersonnelLeaveManagement
```

## Projeleri oluştur
```bash
dotnet new webapi -n PersonnelLeaveManagement.Api
dotnet new classlib -n PersonnelLeaveManagement.Domain
dotnet new classlib -n PersonnelLeaveManagement.Application
dotnet new classlib -n PersonnelLeaveManagement.Infrastructure
```

## Projeleri solution’a ekle
```bash
dotnet sln PersonnelLeaveManagement.sln add PersonnelLeaveManagement.Api/PersonnelLeaveManagement.Api.csproj
dotnet sln PersonnelLeaveManagement.sln add PersonnelLeaveManagement.Domain/PersonnelLeaveManagement.Domain.csproj
dotnet sln PersonnelLeaveManagement.sln add PersonnelLeaveManagement.Application/PersonnelLeaveManagement.Application.csproj
dotnet sln PersonnelLeaveManagement.sln add PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj
```

## Proje referanslarını bağla (katmanlı yapı)

Şimdi referanslar:

- API → Application & Infrastructure

- Application → Domain

- Infrastructure → Domain

```bash
dotnet add PersonnelLeaveManagement.Api/PersonnelLeaveManagement.Api.csproj reference PersonnelLeaveManagement.Application/PersonnelLeaveManagement.Application.csproj
dotnet add PersonnelLeaveManagement.Api/PersonnelLeaveManagement.Api.csproj reference PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj

dotnet add PersonnelLeaveManagement.Application/PersonnelLeaveManagement.Application.csproj reference PersonnelLeaveManagement.Domain/PersonnelLeaveManagement.Domain.csproj

dotnet add PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj reference PersonnelLeaveManagement.Domain/PersonnelLeaveManagement.Domain.csproj
```

Sonra bir test run:
```bash
dotnet build
```

# EF Core + MSSQL Hazırlığı
src klasöründesin
```bash
cd src
```

## Infrastructure projesine EF Core + SQL Server + Tools
```bash
dotnet add PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
```

Api projesine de Design paketini ekle (migrations için genelde API projesi başlangıç projesi oluyor):

```bash
dotnet add PersonnelLeaveManagement.Api/PersonnelLeaveManagement.Api.csproj package Microsoft.EntityFrameworkCore.Design
```