namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

public class CreateHeroSummary : Summary<CreateHeroEndpoint>
{
    public CreateHeroSummary()
    {
        Summary = "Create a new hero";
        Description = "Creates a new hero with the specified name, alias, and powers. Returns the ID of the created hero.";

        // Request example
        ExampleRequest = new CreateHeroRequest(
            Name: "Peter Parker",
            Alias: "Spider-Man",
            Powers:
            [
                new CreateHeroRequest.HeroPowerDto("Web Slinging", 1),
                new CreateHeroRequest.HeroPowerDto("Spider Sense", 7),
                new CreateHeroRequest.HeroPowerDto("Wall Crawling", 10)
            ]);

        // Also, add response examples if needed
    }
}
