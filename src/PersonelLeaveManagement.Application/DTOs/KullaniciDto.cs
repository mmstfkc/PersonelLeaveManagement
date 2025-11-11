using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelLeaveManagement.Application.DTOs;

public class KullaniciLoginDto
{
    public string Email { get; set; }
    public string Sifre { get; set; }
}

public class KullaniciRegisterDto
{
    public string KullaniciAdi { get; set; }
    public string Email { get; set; }
    public string Sifre { get; set; }
    public string Rol {  get; set; }
}
