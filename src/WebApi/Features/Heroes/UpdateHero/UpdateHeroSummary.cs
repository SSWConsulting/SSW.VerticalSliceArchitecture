namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

public class UpdateHeroSummary : Summary<UpdateHeroEndpoint>
{
    public UpdateHeroSummary()
    {
        Summary = "Update an existing hero";
        Description = "Updates a hero's name, alias, and powers. The hero must exist.";

        // Request example
        ExampleRequest = new UpdateHeroRequest(
            Name: "Peter Benjamin Parker",
            Alias: "The Amazing Spider-Man",
            HeroId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Powers:
            [
                new UpdateHeroRequest.HeroPowerDto("Web Slinging", 90),
                new UpdateHeroRequest.HeroPowerDto("Spider Sense", 95),
                new UpdateHeroRequest.HeroPowerDto("Wall Crawling", 98),
                new UpdateHeroRequest.HeroPowerDto("Super Strength", 75)
            ]);

        // Also, add response examples if needed
    }
}
