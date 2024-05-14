using NetArchTest.Rules;
using System.Diagnostics;

namespace VerticalSliceArchitecture.ArchTests;

public abstract class TestBase
{
    private static readonly Types ProgramTypes = Types.InAssembly(typeof(Program).Assembly);

    protected PredicateList TypesMatchingAnyPattern(params string[] patterns)
    {
        var output = ProgramTypes.That();

        for (var index = 0; index < patterns.Length; index++)
        {
            var pattern = patterns[index];

            if (index == patterns.Length - 1)
            {
                return output.ResideInNamespaceContaining(pattern);
            }

            output = output.ResideInNamespaceContaining(pattern).Or();
        }

        throw new UnreachableException();
    }
}
