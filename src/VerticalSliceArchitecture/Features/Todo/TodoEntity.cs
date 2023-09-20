namespace VerticalSliceArchitecture.Features.Todo;

public class TodoEntity
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Completed { get; set; }
}