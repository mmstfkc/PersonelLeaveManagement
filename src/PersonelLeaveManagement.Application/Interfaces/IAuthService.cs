using PersonelLeaveManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelLeaveManagement.Application.Interfaces;
public interface IAuthService
{
    Task<string?> LoginAsync(KullaniciLoginDto dto);
    Task<bool> RegisterAsync(KullaniciRegisterDto dto); 
}
