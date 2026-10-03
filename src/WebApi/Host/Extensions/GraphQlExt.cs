using SSW.VerticalSliceArchitecture.Common.GraphQL.Scalars;
using SSW.VerticalSliceArchitecture.Common.Persistence;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Host.Extensions;

public static class GraphQlExt
{
    public static void AddCustomGraphQL(this IHostApplicationBuilder builder)
    {
        builder.Services.AddWebApiDataLoaders();

        builder
            .AddGraphQL()

            // Source generated from every [QueryType], [MutationType], [SubscriptionType],
            // [ObjectType<T>] and [DataLoader] in this assembly. A new slice joins the schema by
            // existing, with no central list to edit.
            .AddWebApiTypes()

            // Turns a resolver's parameters into an Input type and its return value into a Payload
            // type, and puts the errors declared with [Error<T>] into that payload.
            .AddMutationConventions()

            // Paging, filtering and sorting middleware for the list queries. The allow-lists are
            // per feature: see HeroFilterType and TeamFilterType.
            .AddFiltering()
            .AddSorting()
            .ModifyPagingOptions(options =>
            {
                options.DefaultPageSize = 10;
                options.MaxPageSize = 50;
                options.IncludeTotalCount = true;
            })

            // TODO: an in-memory provider keeps subscriptions to one process. Swap it for Redis or
            // Azure Service Bus before running more than one instance.
            .AddInMemorySubscriptions()

            // Each resolver that declares an ApplicationDbContext parameter gets its own instance,
            // which is what makes parallel field resolution safe. See Common/Persistence.
            .RegisterDbContextFactory<ApplicationDbContext>()

            // Without an explicit binding HotChocolate infers an object type from the Vogen struct
            // and the schema fails to build on the duplicate name.
            .BindRuntimeType<HeroId, HeroIdType>()
            .BindRuntimeType<TeamId, TeamIdType>()
            .BindRuntimeType<MissionId, MissionIdType>();
    }
}
