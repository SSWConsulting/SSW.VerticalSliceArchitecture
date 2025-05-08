namespace VerticalSliceArchitectureTemplate.Common.Domain.Todos;

public class TodoErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Todo.NotFound",
        "Todo is not found");
}