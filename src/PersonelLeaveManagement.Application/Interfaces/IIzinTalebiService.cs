using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonelLeaveManagement.Application.DTOs;

namespace PersonelLeaveManagement.Application.Interfaces;

public interface IIzinTalebiService
{
    Task<IEnumerable<IzinTalebiDto>> GetAllAsync();
    Task<IzinTalebiDto> GetByIdAsync(int id);
    Task<IzinTalebiDto> CreateAsync(IzinTalebiDto dto);
    Task<bool> UpdateAsync(int id,  IzinTalebiDto dto);
    Task<bool> DeleteAsync(int id);
}