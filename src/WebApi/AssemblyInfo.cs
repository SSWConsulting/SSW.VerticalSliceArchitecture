// The two module names below decide the extension methods the HotChocolate source generator
// emits: AddWebApiTypes() collects every [QueryType], [MutationType], [SubscriptionType] and
// [ObjectType<T>] in this assembly, and AddWebApiDataLoaders() every [DataLoader] method.
// Without these attributes the generator emits nothing and the schema comes up empty.
[assembly: Module("WebApiTypes")]
[assembly: DataLoaderModule("WebApiDataLoaders")]
