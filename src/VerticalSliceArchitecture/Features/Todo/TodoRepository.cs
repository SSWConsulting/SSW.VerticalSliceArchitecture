namespace VerticalSliceArchitecture.Features.Todo;

public interface ITodoRepository
{
    Task<IEnumerable<TodoEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TodoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TodoEntity> AddAsync(TodoEntity todo, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoEntity todo, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoEntity todo, CancellationToken cancellationToken = default);
}

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _appDbContext;

    public TodoRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public async Task<IEnumerable<TodoEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Todos.ToListAsync(cancellationToken: cancellationToken);
    }
    
    public async Task<TodoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Todos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<TodoEntity> AddAsync(TodoEntity todo, CancellationToken cancellationToken = default)
    {
        _appDbContext.Todos.Add(todo);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return todo;
    }
    
    public async Task DeleteAsync(TodoEntity todo, CancellationToken cancellationToken = default)
    {
        _appDbContext.Todos.Remove(todo);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateAsync(TodoEntity todo, CancellationToken cancellationToken = default)
    {
        _appDbContext.Todos.Update(todo);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}