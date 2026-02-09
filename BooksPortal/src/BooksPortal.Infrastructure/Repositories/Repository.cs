using System.Linq.Expressions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Domain.Common;
using BooksPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly BooksPortalDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(BooksPortalDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        => await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(cancellationToken);

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public void Add(T entity) => _dbSet.Add(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        _dbSet.Update(entity);
    }

    public void HardDelete(T entity) => _dbSet.Remove(entity);

    public void HardDeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public IQueryable<T> QueryIgnoringFilters() => _dbSet.IgnoreQueryFilters().AsQueryable();
}
