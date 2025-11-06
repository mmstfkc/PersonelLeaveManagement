using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonelLeaveManagement.Application.DTOs;

namespace PersonelLeaveManagement.Application.Interfaces;

public interface IPersonelService
{
    Task<IEnumerable<PersonelDto>> GetAllAsync();
    Task<PersonelDto> GetByIdAsync(int id);
    Task<PersonelDto> CreateAsync(PersonelDto dto);
    Task<bool> UpdateAsync(int id, PersonelDto dto);
    Task<bool> DeleteAsync(int id);
}