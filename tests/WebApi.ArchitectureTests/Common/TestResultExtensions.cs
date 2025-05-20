using FluentAssertions.Execution;
using TestResult = NetArchTest.Rules.TestResult;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests.Common;

public static class TestResultExtensions
{
    public static void DumpFailingTypes(this TestResult result, ITestOutputHelper outputHelper)
    {
        if (result.IsSuccessful)
            return;

        outputHelper.WriteLine("Failing Types:");

        foreach (var type in result.FailingTypes)
        {
            if (type.FullName is null)
                continue;

            outputHelper.WriteLine(type.FullName);
        }
    }

    public static TestResultAssertions Should(this TestResult result) => new(result, AssertionChain.GetOrCreate());
}