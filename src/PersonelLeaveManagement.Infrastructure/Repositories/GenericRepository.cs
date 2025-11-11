using Microsoft.EntityFrameworkCore;
using PersonelLeaveManagement.Application.Interfaces;
using PersonelLeaveManagement.Infrastructure.Persistence;
using System.Linq.Expressions;


namespace PersonelLeaveManagement.Infrastructure.Repositories;

public class GenericRepository<T>: IGenericRepository<T> where T: class
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).AsNoTracking().ToListAsync();

    public async Task  AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Remove(T entity) =>  _dbSet.Remove(entity);

    public async Task<int> SaveChanges() => await _context.SaveChangesAsync();

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}