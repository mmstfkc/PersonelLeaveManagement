using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Repositories;

namespace PersonelLeaveManagement.Infrastructure.Services;

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
            SoyAd = x.Soyad,
            TCKimlikNo = x.TCKimlikNo,
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
        entity.Soyad = dto.SoyAd;
        entity.TCKimlikNo = dto.TCKimlikNo;
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