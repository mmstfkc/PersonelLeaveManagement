using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Repositories;

namespace PersonelLeaveManagement.Infrastructure.Services;

public class IzinTalebiService : IIzinTalebiService
{
    private readonly IGenericRepository<IzinTalebi> _repository;

    public IzinTalebiService(IGenericRepository<IzinTalebi> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<IzinTalebiDto>> GetAllAsync()
    {
        var data = await _repository.GetAllAsync();
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
        var x = await _repository.GetByIdAsync(id);
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

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(int id, IzinTalebiDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.BaslangicTarihi = dto.BaslangicTarihi;
        entity.BitisTarihi = dto.BitisTarihi;
        entity.Durum = dto.Durum;

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