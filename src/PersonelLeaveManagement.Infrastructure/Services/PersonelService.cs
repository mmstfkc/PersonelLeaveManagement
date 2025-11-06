using Microsoft.EntityFrameworkCore;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Persistence;

namespace PersonelLeaveManagement.Infrastructure.Services;

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
            SoyAd = x.Soyad,
            TCKimlikNo = x.TCKimlikNo,
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
            SoyAd = x.Soyad,
            TCKimlikNo = x.TCKimlikNo,
            IseGirisTarihi = x.IseGirisTarihi
        };
    }

    public async Task<PersonelDto> CreateAsync(PersonelDto dto)
    {
        var entity = new Personel
        {
            Ad = dto.Ad,
            Soyad = dto.SoyAd,
            TCKimlikNo = dto.TCKimlikNo,
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
        entity.Soyad = dto.SoyAd;
        entity.TCKimlikNo = dto.TCKimlikNo;
        entity.IseGirisTarihi = dto.IseGirisTarihi;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Personeller.FindAsync(id);
        if (entity == null) return false;

        _context.Personeller.Remove(entity);
        return true;
    }
}