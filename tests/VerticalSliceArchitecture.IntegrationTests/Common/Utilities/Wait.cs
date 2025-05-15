namespace VerticalSliceArchitectureTemplate.IntegrationTests.Common.Utilities;

internal static class Wait
{
    private const int Milliseconds = 1000;

    /// <summary>
    /// Add a delay to allow the event to be processed
    /// </summary>
    internal static async Task ForEventualConsistency()
    {
        await Task.Delay(Milliseconds);
    }
}