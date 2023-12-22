using VerticalSliceArchitectureTemplate.Common;

namespace VerticalSliceArchitectureTemplate.Features.Todos;

public interface ITodoRepository
{
    Task<IEnumerable<Todo>> GetAllAsync(bool? isCompleted = null, CancellationToken cancellationToken = default);
    Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Todo todo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default);
}

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _appDbContext;

    public TodoRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public async Task<IEnumerable<Todo>> GetAllAsync(bool? isCompleted = null, CancellationToken cancellationToken = default)
    {
        var query = _appDbContext.Todos.AsQueryable();

        if (isCompleted is not null)
        {
            query = query.Where(x => x.Completed == isCompleted);
        }
        
        return await query.ToListAsync(cancellationToken: cancellationToken);
    }
    
    public async Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Todos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _appDbContext.Todos.Add(todo);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return todo;
    }
    
    public async Task DeleteAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _appDbContext.Todos.Remove(todo);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _appDbContext.Todos.Update(todo);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}