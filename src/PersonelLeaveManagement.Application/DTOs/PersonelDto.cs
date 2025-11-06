using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelLeaveManagement.Application.DTOs;

public class PersonelDto
{
    public int Id { get; set; }
    public string Ad { get; set; }
    public string SoyAd { get; set; }
    public string TCKimlikNo { get; set; }
    public DateTime IseGirisTarihi { get; set; }
}
