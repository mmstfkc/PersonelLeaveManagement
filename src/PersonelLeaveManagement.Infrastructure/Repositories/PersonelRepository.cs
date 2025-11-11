using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Persistence;

namespace PersonelLeaveManagement.Infrastructure.Repositories;

public class PersonelRepository: GenericRepository<Personel>
{
    public PersonelRepository(AppDbContext _context) : base(_context) { }
}
 
