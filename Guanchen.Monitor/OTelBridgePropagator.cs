/*
This whole file is copied from https://github.com/open-telemetry/opentelemetry-dotnet/issues/5667
*/

using System.Collections.ObjectModel;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

class OTelBridgePropagator : DistributedContextPropagator
{
    private readonly TextMapPropagator _inner;
    public OTelBridgePropagator(TextMapPropagator propagator)
    {
        _inner = propagator;
        Fields = new ReadOnlyCollection<string>(_inner.Fields.ToArray());
    }

    public override IReadOnlyCollection<string> Fields { get; }

    private PropagationContext Extract(object? carrier, PropagatorGetterCallback getter)
    {
        return _inner.Extract(default, carrier, (c, n) =>
        {
            getter.Invoke(c, n, out var value, out var values);
            return values;
        });

    }
    public override IEnumerable<KeyValuePair<string, string?>>? ExtractBaggage(object? carrier, PropagatorGetterCallback? getter)
    {
        if (getter == null)
        {
            return null;
        }

        return Extract(carrier, getter).Baggage.GetBaggage();
    }

    public override void ExtractTraceIdAndState(object? carrier, PropagatorGetterCallback? getter, out string? traceParent, out string? traceState)
    {
        traceParent = null;
        traceState = null;

        if (getter != null)
        {
            var context = Extract(carrier, getter);
            var flags = (context.ActivityContext.TraceFlags == ActivityTraceFlags.Recorded) ? "01" : "00";
            traceParent = $"00-{context.ActivityContext.TraceId}-{context.ActivityContext.SpanId}-{flags}";
            traceState = context.ActivityContext.TraceState;
        }
    }

    public override void Inject(Activity? activity, object? carrier, PropagatorSetterCallback? setter)
    {
        if (setter == null)
        {
            return;
        }

        if (activity != null)
        {
            foreach (var kvp in activity.Baggage)
            {
                Baggage.Current.SetBaggage(kvp.Key, kvp.Value);
            }
        }

        var context = new PropagationContext(activity?.Context ?? default, Baggage.Current);
        _inner.Inject(context, carrier, setter.Invoke);
    }
}