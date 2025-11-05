using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelLeaveManagement.Domain.Entities;

public class Personel
{
    public int Id { get; set; }
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string TCKimlikNo { get; set; } = null!;
    public DateTime IseGirisTarihi { get; set; }

    public ICollection<IzinTalebi> IzinTalepleri { get; set; } = new List<IzinTalebi>(); 
}

public class IzinTalebi
{
    public int Id { get; set; }
    public int PersonelId { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string Durum { get; set; } = "Beklemede";
    public Personel? Personel { get; set; }
}

