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
