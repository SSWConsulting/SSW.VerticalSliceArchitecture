using SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.Heroes;

public class GetAllHeroesRequestValidatorTests
{
    private readonly GetAllHeroesRequestValidator _validator = new();

    [Theory]
    [InlineData("name")]
    [InlineData("alias")]
    [InlineData("powerLevel")]
    [InlineData("POWERLEVEL")]
    [InlineData(null)]
    public void Validator_WithAllowedSortColumn_ShouldPass(string? sortBy)
    {
        // Act
        var result = _validator.Validate(new GetAllHeroesRequest { SortBy = sortBy });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_WithUnknownSortColumn_ShouldFailAndListTheAllowedColumns()
    {
        // Act
        var result = _validator.Validate(new GetAllHeroesRequest { SortBy = "createdAt" });

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(GetAllHeroesRequest.SortBy)).Subject;
        error.ErrorMessage.Should().Contain("name").And.Contain("alias").And.Contain("powerLevel");
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("DESC")]
    [InlineData("ascending")]
    [InlineData(null)]
    public void Validator_WithAllowedSortDirection_ShouldPass(string? sortDirection)
    {
        // Act
        var result = _validator.Validate(new GetAllHeroesRequest { SortDirection = sortDirection });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_WithUnknownSortDirection_ShouldFail()
    {
        // Act
        var result = _validator.Validate(new GetAllHeroesRequest { SortDirection = "sideways" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllHeroesRequest.SortDirection));
    }

    // Out-of-range paging is clamped by PagingParams rather than rejected, so the validator must let it
    // through — a rule added here would turn a clamped request into a 400.
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-5, 5000)]
    public void Validator_WithOutOfRangePaging_ShouldPass(int page, int pageSize)
    {
        // Act
        var result = _validator.Validate(new GetAllHeroesRequest { Page = page, PageSize = pageSize });

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
