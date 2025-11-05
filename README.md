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

# İlk Entity ve DbContext
## Domain: Personel ve IzinTalebi entity’leri
PersonnelLeaveManagement.Domain projesinde, örneğin Entities diye bir klasör aç ve içine iki class koy.

Personel
```c#
namespace PersonnelLeaveManagement.Domain.Entities;

public class Personel
{
    public int Id { get; set; }
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string TcKimlikNo { get; set; } = null!;
    public DateTime IseGirisTarihi { get; set; }

    public ICollection<IzinTalebi> IzinTalepleri { get; set; } = new List<IzinTalebi>();
}
```
IzinTalebi
```c#
namespace PersonnelLeaveManagement.Domain.Entities;

public class IzinTalebi
{
    public int Id { get; set; }
    public int PersonelId { get; set; }

    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }

    // Beklemede, Onaylandi, Reddedildi
    public string Durum { get; set; } = "Beklemede";

    public Personel? Personel { get; set; }
}
```

## Infrastructure: AppDbContext
PersonnelLeaveManagement.Infrastructure projesinde Persistence adında bir klasör aç ve içine AppDbContext.cs ekle
```c#
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveManagement.Domain.Entities;

namespace PersonnelLeaveManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Personel> Personeller { get; set; }
    public DbSet<IzinTalebi> IzinTalepleri { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Personel>(entity =>
        {
            entity.ToTable("Personeller");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Ad).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Soyad).IsRequired().HasMaxLength(100);
            entity.Property(p => p.TcKimlikNo).IsRequired().HasMaxLength(11);

            entity.HasMany(p => p.IzinTalepleri)
                  .WithOne(i => i.Personel)
                  .HasForeignKey(i => i.PersonelId);
        });

        modelBuilder.Entity<IzinTalebi>(entity =>
        {
            entity.ToTable("IzinTalepleri");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Durum)
                  .IsRequired()
                  .HasMaxLength(20);
        });
    }
}
```

Bu sayede:

- EF Core ile iki tablo: Personeller ve IzinTalepleri
- 1-n ilişki: bir Personel’in birden çok izin talebi.

## API Projesinde DbContext’i Register Etme (DI)

PersonnelLeaveManagement.Api projesinde appsettings.json içine bir connection string ekleyelim

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PersonnelLeaveDb;User Id=sa;Password=Str0ng_Pass!;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

swagger ui i projemize ekliyoruz
```bash
dotnet add PersonnelLeaveManagement.Api package Swashbuckle.AspNetCore
```


Sonra Program.cs içinde (ASP.NET Core 6/7/8 minimal hosting model)

```c#
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// DbContext kaydı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Migration ve Veritabanı Oluşturma

### dotnet-ef CLI kurulum
```bash
dotnet tool install --global dotnet-ef
```
Kurulumu doğrula

```bash
dotnet ef --version
```

### Mİgrationları oluşturma ve dbyi hazır hale getirme
src klasöründeyken, migrations almak için (başlangıç projesi API olsun):

```bash
dotnet ef migrations add InitialCreate -p PersonnelLeaveManagement.Infrastructure -s PersonnelLeaveManagement.Api
```

- -p → Migrations’ı hangi projeye yazacağını söylüyoruz (Infrastructure)
- -s → Startup/host projesi (Api)

Sonra veritabanını oluştur
```bash
dotnet ef database update -p PersonnelLeaveManagement.Infrastructure -s PersonnelLeaveManagement.Api
```
