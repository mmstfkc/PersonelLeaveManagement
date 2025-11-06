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

dotnet add PersonnelLeaveManagement.Infrastructure/PersonnelLeaveManagement.Infrastructure.csproj reference PersonnelLeaveManagement.Application/PersonnelLeaveManagement.Application.csproj

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

# CRUD (Controller + Service + DTO) katmaını kurmak

## Application katmanında DTOlar

PersonnelLeaveManagement.Application projesinde DTOs adında bir klasör aç.
İçine iki DTO koy:

PersonelDto.cs
```c#
namespace PersonnelLeaveManagement.Application.DTOs;

public class PersonelDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string TcKimlikNo { get; set; } = null!;
    public DateTime IseGirisTarihi { get; set; }
}
```

IzinTalebiDto.cs
```c#
namespace PersonnelLeaveManagement.Application.DTOs;

public class IzinTalebiDto
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string Durum { get; set; } = null!;
}
```

## Application Katmanında Servis Arayüzleri

IPersonelService.cs

```c#
using PersonnelLeaveManagement.Application.DTOs;

namespace PersonnelLeaveManagement.Application.Interfaces;

public interface IPersonelService
{
    Task<IEnumerable<PersonelDto>> GetAllAsync();
    Task<PersonelDto?> GetByIdAsync(int id);
    Task<PersonelDto> CreateAsync(PersonelDto dto);
    Task<bool> UpdateAsync(int id, PersonelDto dto);
    Task<bool> DeleteAsync(int id);
}
```

IIzinTalebiService.cs
```c#
using PersonnelLeaveManagement.Application.DTOs;

namespace PersonnelLeaveManagement.Application.Interfaces;

public interface IIzinTalebiService
{
    Task<IEnumerable<IzinTalebiDto>> GetAllAsync();
    Task<IzinTalebiDto?> GetByIdAsync(int id);
    Task<IzinTalebiDto> CreateAsync(IzinTalebiDto dto);
    Task<bool> UpdateAsync(int id, IzinTalebiDto dto);
    Task<bool> DeleteAsync(int id);
}
```

## Infrastructure Katmanında Servislerin Gerçekleştirilmesi
PersonnelLeaveManagement.Infrastructure projesinde Services adlı klasör oluştur.

PersonelService.cs
```c#
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveManagement.Application.DTOs;
using PersonnelLeaveManagement.Application.Interfaces;
using PersonnelLeaveManagement.Domain.Entities;
using PersonnelLeaveManagement.Infrastructure.Persistence;

namespace PersonnelLeaveManagement.Infrastructure.Services;

public class PersonelService : IPersonelService
{
    private readonly AppDbContext _context;

    public PersonelService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PersonelDto>> GetAllAsync()
    {
        var data = await _context.Personeller.AsNoTracking().ToListAsync();
        return data.Select(x => new PersonelDto
        {
            Id = x.Id,
            Ad = x.Ad,
            Soyad = x.Soyad,
            TcKimlikNo = x.TcKimlikNo,
            IseGirisTarihi = x.IseGirisTarihi
        });
    }

    public async Task<PersonelDto?> GetByIdAsync(int id)
    {
        var x = await _context.Personeller.FindAsync(id);
        if (x == null) return null;
        return new PersonelDto
        {
            Id = x.Id,
            Ad = x.Ad,
            Soyad = x.Soyad,
            TcKimlikNo = x.TcKimlikNo,
            IseGirisTarihi = x.IseGirisTarihi
        };
    }

    public async Task<PersonelDto> CreateAsync(PersonelDto dto)
    {
        var entity = new Personel
        {
            Ad = dto.Ad,
            Soyad = dto.Soyad,
            TcKimlikNo = dto.TcKimlikNo,
            IseGirisTarihi = dto.IseGirisTarihi
        };

        _context.Personeller.Add(entity);
        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(int id, PersonelDto dto)
    {
        var entity = await _context.Personeller.FindAsync(id);
        if (entity == null) return false;

        entity.Ad = dto.Ad;
        entity.Soyad = dto.Soyad;
        entity.TcKimlikNo = dto.TcKimlikNo;
        entity.IseGirisTarihi = dto.IseGirisTarihi;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Personeller.FindAsync(id);
        if (entity == null) return false;

        _context.Personeller.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
```
IzinTalebiService.cs
```c#
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveManagement.Application.DTOs;
using PersonnelLeaveManagement.Application.Interfaces;
using PersonnelLeaveManagement.Domain.Entities;
using PersonnelLeaveManagement.Infrastructure.Persistence;

namespace PersonnelLeaveManagement.Infrastructure.Services;

public class IzinTalebiService : IIzinTalebiService
{
    private readonly AppDbContext _context;

    public IzinTalebiService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IzinTalebiDto>> GetAllAsync()
    {
        var data = await _context.IzinTalepleri.AsNoTracking().ToListAsync();
        return data.Select(x => new IzinTalebiDto
        {
            Id = x.Id,
            PersonelId = x.PersonelId,
            BaslangicTarihi = x.BaslangicTarihi,
            BitisTarihi = x.BitisTarihi,
            Durum = x.Durum
        });
    }

    public async Task<IzinTalebiDto?> GetByIdAsync(int id)
    {
        var x = await _context.IzinTalepleri.FindAsync(id);
        if (x == null) return null;
        return new IzinTalebiDto
        {
            Id = x.Id,
            PersonelId = x.PersonelId,
            BaslangicTarihi = x.BaslangicTarihi,
            BitisTarihi = x.BitisTarihi,
            Durum = x.Durum
        };
    }

    public async Task<IzinTalebiDto> CreateAsync(IzinTalebiDto dto)
    {
        var entity = new IzinTalebi
        {
            PersonelId = dto.PersonelId,
            BaslangicTarihi = dto.BaslangicTarihi,
            BitisTarihi = dto.BitisTarihi,
            Durum = dto.Durum
        };

        _context.IzinTalepleri.Add(entity);
        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(int id, IzinTalebiDto dto)
    {
        var entity = await _context.IzinTalepleri.FindAsync(id);
        if (entity == null) return false;

        entity.BaslangicTarihi = dto.BaslangicTarihi;
        entity.BitisTarihi = dto.BitisTarihi;
        entity.Durum = dto.Durum;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.IzinTalepleri.FindAsync(id);
        if (entity == null) return false;

        _context.IzinTalepleri.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
```
## API Katmanında Servisleri Register Et
Program.cs içine şu satırları ekle (DbContext kaydının altına):

```c#
using PersonnelLeaveManagement.Application.Interfaces;
using PersonnelLeaveManagement.Infrastructure.Services;

builder.Services.AddScoped<IPersonelService, PersonelService>();
builder.Services.AddScoped<IIzinTalebiService, IzinTalebiService>();
```

## API Controller’ları
PersonnelLeaveManagement.Api/Controllers klasöründe iki controller oluştur.

PersonelController.cs
```c#
using Microsoft.AspNetCore.Mvc;
using PersonnelLeaveManagement.Application.DTOs;
using PersonnelLeaveManagement.Application.Interfaces;

namespace PersonnelLeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonelController : ControllerBase
{
    private readonly IPersonelService _service;

    public PersonelController(IPersonelService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PersonelDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PersonelDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
```


IzinTalebiController.cs
```c#
using Microsoft.AspNetCore.Mvc;
using PersonnelLeaveManagement.Application.DTOs;
using PersonnelLeaveManagement.Application.Interfaces;

namespace PersonnelLeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IzinTalebiController : ControllerBase
{
    private readonly IIzinTalebiService _service;

    public IzinTalebiController(IIzinTalebiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IzinTalebiDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] IzinTalebiDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
```

## Test Et
Şimdi projenin kök dizininde:
```c#
dotnet build
dotnet run --project .\PersonnelLeaveManagement.Api
```

Swagger açılmalı:
➡️ https://localhost:5001/swagger veya http://localhost:5000/swagger

Orada:

- /api/Personel
- /api/IzinTalebi

endpoint’leri listelenmiş olmalı.
Yeni personel ekleyip izin talebi oluşturabilirsin.