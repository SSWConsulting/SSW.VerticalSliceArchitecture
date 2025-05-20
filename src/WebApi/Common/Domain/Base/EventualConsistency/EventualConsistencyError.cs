namespace SSW.VerticalSliceArchitecture.Common.Domain.Base.EventualConsistency;

public static class EventualConsistencyError
{
    public const int EventualConsistencyType = 100;

    public static Error From(string code, string description)
    {
        return Error.Custom(EventualConsistencyType, code, description);
    }
}