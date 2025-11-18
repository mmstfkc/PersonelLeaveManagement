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

# Kurumsal Seviyeye Geçiş
(Repository Pattern + Validation + Global Exception Middleware)

Bu aşamada amacımız:
- ✅ Kodun kurumsal standartlara uygun hale gelmesi
- ✅ Validation (veri kontrolü) eklendiğinde API’nin güvenilirleşmesi
- ✅ Global exception middleware ile hataların kontrol altına alınması
- ✅ Repository pattern ile data erişiminin soyutlanması

## Repository Pattern Yapısı
Şu anda servisler doğrudan AppDbContext kullanıyor.
Biz bunu Repository aracılığıyla soyutlayacağız.

📁 Infrastructure katmanında yeni klasör oluştur:

Repositories

### IGenericRepository.cs → Application katmanına (Interfaces altına)
```c#
using System.Linq.Expressions;

namespace PersonnelLeaveManagement.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}
```

### GenericRepository.cs → Infrastructure/Repositories altına
```c#
using Microsoft.EntityFrameworkCore;
using PersonnelLeaveManagement.Application.Interfaces;
using PersonnelLeaveManagement.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace PersonnelLeaveManagement.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).AsNoTracking().ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
```

### Somut repository’ler → Infrastructure/Repositories altına

PersonelRepository.cs
```c#
using PersonnelLeaveManagement.Domain.Entities;
using PersonnelLeaveManagement.Infrastructure.Persistence;

namespace PersonnelLeaveManagement.Infrastructure.Repositories;

public class PersonelRepository : GenericRepository<Personel>
{
    public PersonelRepository(AppDbContext context) : base(context)
    {
    }
}
```

IzinTalebiRepository.cs
```c#
using PersonnelLeaveManagement.Domain.Entities;
using PersonnelLeaveManagement.Infrastructure.Persistence;

namespace PersonnelLeaveManagement.Infrastructure.Repositories;

public class IzinTalebiRepository : GenericRepository<IzinTalebi>
{
    public IzinTalebiRepository(AppDbContext context) : base(context)
    {
    }
}
```

## Servisleri Repository’e göre güncelle
Şimdi PersonelService ve IzinTalebiService artık _context yerine _repository kullanacak.

PersonelService.cs
```c#
using PersonnelLeaveManagement.Application.DTOs;
using PersonnelLeaveManagement.Application.Interfaces;
using PersonnelLeaveManagement.Domain.Entities;

namespace PersonnelLeaveManagement.Infrastructure.Services;

public class PersonelService : IPersonelService
{
    private readonly IGenericRepository<Personel> _repository;

    public PersonelService(IGenericRepository<Personel> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PersonelDto>> GetAllAsync()
    {
        var data = await _repository.GetAllAsync();
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
        var x = await _repository.GetByIdAsync(id);
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

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(int id, PersonelDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Ad = dto.Ad;
        entity.Soyad = dto.Soyad;
        entity.TcKimlikNo = dto.TcKimlikNo;
        entity.IseGirisTarihi = dto.IseGirisTarihi;

        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();
        return true;
    }
}
```
IzinTalebiService için de aynı şekilde IGenericRepository<IzinTalebi> enjekte edeceğiz.

## Repository’leri DI (Dependency Injection) içine kaydet
Program.cs dosyasına şu satırı ekle:

```c#
using PersonnelLeaveManagement.Application.Interfaces;
using PersonnelLeaveManagement.Infrastructure.Repositories;

// Repository kayıtları
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
```
Artık servisler _context yerine repository kullanacak şekilde otomatik bağlanıyor.


# FluentValidation ile Validation Katmanı
Paket kur
```bash
dotnet add PersonnelLeaveManagement.Api package FluentValidation.AspNetCore
```

## Application katmanına Validators klasörü aç
PersonelValidator.cs
```c#
using FluentValidation;
using PersonnelLeaveManagement.Application.DTOs;

namespace PersonnelLeaveManagement.Application.Validators;

public class PersonelValidator : AbstractValidator<PersonelDto>
{
    public PersonelValidator()
    {
        RuleFor(x => x.Ad).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Soyad).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TcKimlikNo)
            .NotEmpty().Length(11)
            .Matches("^[0-9]+$").WithMessage("TCKN sadece rakamlardan oluşmalıdır.");
    }
}
```

## Program.cs’de FluentValidation ekle
```c#
using FluentValidation.AspNetCore;
using PersonnelLeaveManagement.Application.Validators;

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<PersonelValidator>());
```

# Global Exception Middleware
Infrastructure veya Api katmanında Middlewares klasörü aç

ExceptionMiddleware.cs
```c#
using System.Net;
using System.Text.Json;

namespace PersonnelLeaveManagement.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata oluştu: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                Message = "Sunucu hatası oluştu.",
                Detail = ex.Message
            });
            await context.Response.WriteAsync(result);
        }
    }
}
```

## Program.cs’ye ekle
```c#
using PersonnelLeaveManagement.Api.Middlewares;

app.UseMiddleware<ExceptionMiddleware>();
```

Şimdi test zamanı

Swagger’da POST /api/Personel çağır:

Ad boş gönder → 400 döner.

TcKimlikNo 9 haneli gönder → 400 döner (validator çalışıyor).

Rastgele exception fırlatmak için bir metodda throw new Exception("Test"); dersen,
Middleware bunu yakalayıp { "Message": "Sunucu hatası oluştu." } döner.

Kod artık repository pattern, validation, global error handling ile kurumsal hale geldi 🎯

# JWT entegrasyonu
## Genel Yapı

JWT (JSON Web Token) sistemi 3 temel parçadan oluşur:

- 1️⃣ User (Kullanıcı) tablosu — Kimlik bilgilerini tutar (email, şifre, rol vs.)
- 2️⃣ AuthService — Login olur, JWT üretir
- 3️⃣ AuthController — /api/Auth/login endpoint’ini dış dünyaya açar

Ek olarak:

- Program.cs içinde JWT ayarlarını (appsettings.json’dan) okuruz.

- [Authorize] attribute’u ile endpoint’leri koruruz.

## Gerekli NuGet Paketleri
```c#
dotnet add PersonnelLeaveManagement.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add PersonnelLeaveManagement.Api package System.IdentityModel.Tokens.Jwt
dotnet add PersonelLeaveManagement.Infrastructure package BCrypt.Net-Next

```
## Domain: Kullanıcı Entity

Domain/Entities/Kullanici.cs
```c#
namespace PersonelLeaveManagement.Domain.Entities;

public class Kullanici
{
    public int Id { get; set; }
    public string KullaniciAdi { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string SifreHash { get; set; } = null!;
    public string Rol { get; set; } = "User";
}
```

## Infrastructure: AppDbContext’e ekle

AppDbContext.cs içine:
```c#
public DbSet<Kullanici> Kullanicilar { get; set; }
```


Ve hemen migration alalım:

``bash
dotnet ef migrations add AddKullanici -p .\PersonelLeaveManagement.Infrastructure -s .\PersonelLeaveManagement.Api
dotnet ef database update -p .\PersonelLeaveManagement.Infrastructure -s .\PersonelLeaveManagement.Api
````

## Application: DTO + Interface
DTO

Application/DTOs/KullaniciDto.cs
```c#
namespace PersonelLeaveManagement.Application.DTOs;

public class KullaniciLoginDto
{
    public string Email { get; set; } = null!;
    public string Sifre { get; set; } = null!;
}

public class KullaniciRegisterDto
{
    public string KullaniciAdi { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Sifre { get; set; } = null!;
    public string Rol { get; set; } = "User";
}
```


Interface

Application/Interfaces/IAuthService.cs
```c#
using PersonelLeaveManagement.Application.DTOs;

namespace PersonelLeaveManagement.Application.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(KullaniciLoginDto dto);
    Task<bool> RegisterAsync(KullaniciRegisterDto dto);
}
```
Infrastructure: AuthService

Infrastructure/Services/AuthService.cs
```c#
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Persistence;

namespace PersonelLeaveManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<bool> RegisterAsync(KullaniciRegisterDto dto)
    {
        if (await _context.Kullanicilar.AnyAsync(x => x.Email == dto.Email))
            return false;

        var kullanici = new Kullanici
        {
            KullaniciAdi = dto.KullaniciAdi,
            Email = dto.Email,
            SifreHash = BCrypt.Net.BCrypt.HashPassword(dto.Sifre),
            Rol = dto.Rol
        };

        _context.Kullanicilar.Add(kullanici);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> LoginAsync(KullaniciLoginDto dto)
    {
        var user = await _context.Kullanicilar.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Sifre, user.SifreHash))
            return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```


## appsettings.json → JWT ayarları

Api/appsettings.json içine ekle:
```json
"Jwt": {
  "Key": "supersecretkey_1234567890",
  "Issuer": "personelapi",
  "Audience": "personelapi_users"
}
```

## Program.cs → JWT yapılandırması

Program.cs içine builder.Build()’den önce:
```c#
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
```


ve aşağıya, middleware sırasına ekle:
```c#
app.UseAuthentication();
app.UseAuthorization();
```

## API Controller

Api/Controllers/AuthController.cs
```c#
using Microsoft.AspNetCore.Mvc;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;

namespace PersonelLeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] KullaniciRegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result ? Ok("Kayıt başarılı") : BadRequest("Bu e-posta zaten kayıtlı");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] KullaniciLoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        if (token == null)
            return Unauthorized("Geçersiz e-posta veya şifre");

        return Ok(new { Token = token });
    }
}
```

## Test

- 1️⃣ Swagger’da /api/Auth/register çağır → yeni kullanıcı oluştur.
- 2️⃣ /api/Auth/login → email/şifre gönder, token döner.
- 3️⃣ Token’ı kopyala, Swagger’da sağ üstte “Authorize” butonuna tıkla →
Bearer <token> formatında yapıştır.
- 4️⃣ Artık [Authorize] ile korunan endpoint’lere erişebilirsin.

Örnek:
````c#
[Authorize]
[HttpGet("secret")]
public IActionResult SecretArea() => Ok("Sadece token'lı kullanıcılar burayı görebilir!");
```


# Rol Bazlı Erişim + Serilog Logging + Ortam Bazlı Yapı (Development / Production)

Bu adım sonrasında API’n şunları yapabiliyor olacak:

- ✅ Kullanıcı rolleri (“Admin”, “User” vb.) ile endpoint koruması
- ✅ Her istek ve hata için dosyaya + konsola loglama
- ✅ Development / Production ortamına göre farklı ayarlar kullanma

## ROL BAZLI ERİŞİM

JWT sistemimizde zaten her kullanıcıda Rol alanı var.
Bu sayede sadece 2 küçük adımda rol bazlı erişim aktif hale geliyor.

🔹 1️⃣ Controller’da örnek kullanım

Herhangi bir controller’a (örneğin PersonelController) ekle:
```c#
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonelLeaveManagement.Application.Interfaces;

namespace PersonelLeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonelController : ControllerBase
{
    private readonly IPersonelService _service;

    public PersonelController(IPersonelService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [Authorize(Roles = "User,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var personel = await _service.GetByIdAsync(id);
        return personel == null ? NotFound() : Ok(personel);
    }
}
```

## SERILOG LOGGING

Artık her isteği ve hatayı dosyaya + konsola loglayacağız.

🔹 1️⃣ NuGet paketi ekle

```c#
dotnet add PersonelLeaveManagement.Api package Serilog.AspNetCore
dotnet add PersonelLeaveManagement.Api package Serilog.Sinks.File
dotnet add PersonelLeaveManagement.Api package Serilog.Sinks.Console
```

##🔹 2️⃣ Program.cs içine ekle (en üstte)
```c#
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
```

Bu, logları hem terminalde hem de /Logs/log-2025-11-11.txt gibi günlük dosyalarda tutar.


## 🔹 3️⃣ ExceptionMiddleware’de log’ları zenginleştir

ExceptionMiddleware.cs içinde şu satır zaten vardı:

```c#
_logger.LogError(ex, "Beklenmeyen hata oluştu: {Message}", ex.Message);
```

Serilog entegre olduğunda bu log otomatik olarak dosyaya ve konsola yazılır ✅

## ORTAM BAZLI AYARLAR (Development / Production)

Artık appsettings.json dosyasını ikiye ayırıyoruz:

- appsettings.Development.json

- appsettings.Production.json


🔹 1️⃣ appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PersonelDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "dev_secret_key_12345",
    "Issuer": "personelapi_dev",
    "Audience": "personelapi_users_dev"
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

🔹 2️⃣ appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=PersonelDB;User Id=sa;Password=ProdPassword!;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "prod_secret_key_45678",
    "Issuer": "personelapi_prod",
    "Audience": "personelapi_users_prod"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

🔹 3️⃣ launchSettings.json kontrolü

Properties/launchSettings.json içinde environment ayarı olmalı:
```json
"profiles": {
  "PersonelLeaveManagement.Api": {
    "commandName": "Project",
    "launchBrowser": true,
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    },
    "applicationUrl": "https://localhost:7234;http://localhost:5030"
  }
}
```

Production’a geçtiğinde bu değişkeni “Production” yapman yeterli.

# Frontend (React)

Projeyi oluşturuyoruz
````bash
npx create-react-app .
```

Gerekli paketleri üklüyoruz
```bash
 npm install axios react-router-dom
```

Ayağa kaldırıyoruz
```bash
npm start
```
