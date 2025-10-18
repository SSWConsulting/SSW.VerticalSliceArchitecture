// using SSW.VerticalSliceArchitecture.Common.Domain.Base.EventualConsistency;
// using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;
// using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
//
// namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;
//
// public class EventualConsistencyPostProcessor : IGlobalPostProcessor
// {
//     public const string DomainEventsKey = "DomainEventsKey";
//
//     public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
//     {
//         var dbContext = context.HttpContext.Resolve<ApplicationDbContext>();
//         var strategy = dbContext.Database.CreateExecutionStrategy();
//         await strategy.ExecuteInTransactionAsync(async () =>
//         {
//             await PublishEvents(context);
//         }, null!);
//     }
//
//     private static async Task PublishEvents(IPostProcessorContext context)
//     {
//         try
//         {
//             if (context.HttpContext.Items.TryGetValue(DomainEventsKey, out var value) &&
//                 value is Queue<IEvent> domainEvents)
//             {
//                 while (domainEvents.TryDequeue(out var nextEvent))
//                 {
//                     // Publish to MediatR event handlers
//                     await nextEvent.PublishAsync();
//                 }
//             }
//         }
//         // ReSharper disable once RedundantCatchClause
//         catch (EventualConsistencyException)
//         {
//             // TODO: handle eventual consistency exception
//             throw;
//         }
//     }
// }