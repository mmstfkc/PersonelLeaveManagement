using Microsoft.EntityFrameworkCore;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Persistence;

namespace PersonelLeaveManagement.Infrastructure.Services;

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
            BaslangicTarihi =x.BaslangicTarihi,
            BitisTarihi=x.BitisTarihi,
            Durum=x.Durum
        });
    }

    public async Task<IzinTalebiDto> GetByIdAsync(int id)
    {
        var x = await _context.IzinTalepleri.FindAsync(id);
        if (x == null) return null;

        return new IzinTalebiDto
        {
            Id = x.Id,
            PersonelId = x.PersonelId,
            BaslangicTarihi=x.BaslangicTarihi,
            BitisTarihi=x.BitisTarihi,
            Durum=x.Durum
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