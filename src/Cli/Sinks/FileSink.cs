using Core.Abstractions;
using Core.Events;

namespace Cli.Sinks;

public sealed class FileSink : INotificationSink
{
    private readonly string _logPath;

    public FileSink(string logPath)
    {
        _logPath = logPath;
    }

    public void Notify(CatalogChangedEventArgs args)
    {
        var line = $"{args.At:HH:mm:ss} {args.Kind,-8} {args.Id} {args.Delta,4:+0;-0} залишок {args.QuantityAfter}";
        File.AppendAllText(_logPath, line + Environment.NewLine);
    }
}