using Core.Abstractions;
using Core.Events;

namespace Cli.Sinks;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs args)
    {
        Console.WriteLine($"{args.At:HH:mm:ss} {args.Kind,-8} {args.Id} {args.Delta,4:+0;-0} залишок {args.QuantityAfter}");
    }
}