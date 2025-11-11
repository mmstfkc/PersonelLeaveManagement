using PersonelLeaveManagement.Domain.Entities;
using PersonelLeaveManagement.Infrastructure.Persistence;

namespace PersonelLeaveManagement.Infrastructure.Repositories;
public  class IzinTalebiRepository: GenericRepository<IzinTalebi>
{
    public IzinTalebiRepository(AppDbContext _context): base(_context) { }
}

