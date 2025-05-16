using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System.Text;
using TestResult = NetArchTest.Rules.TestResult;

namespace SSW.VerticalSliceArchitecture.ArchitectureTests.Common;

public class TestResultAssertions : ReferenceTypeAssertions<TestResult, TestResultAssertions>
{
    private readonly AssertionChain _chain;

    public TestResultAssertions(TestResult instance, AssertionChain chain) : base(instance, chain)
    {
        _chain = chain;
    }

    protected override string Identifier => "TestResult";

    [CustomAssertion]
    public AndConstraint<TestResultAssertions> BeSuccessful(string because = "", params object[] becauseArgs)
    {
        _chain
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject)
            .ForCondition(s => s.IsSuccessful)
            .FailWith(GetFailureMessage());

        return new AndConstraint<TestResultAssertions>(this);
    }

    private string GetFailureMessage()
    {
        if (Subject.IsSuccessful)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("The following types failed the test:");

        foreach (var name in Subject.FailingTypeNames)
            sb.AppendLine(name);

        return sb.ToString();
    }
}