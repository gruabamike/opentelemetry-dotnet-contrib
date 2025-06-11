// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using MQTTnet.Packets;
using OpenTelemetry.Context.Propagation;

namespace OpenTelemetry.Instrumentation.MqttNetClient;

internal static class MqttClientContextPropagationHandler
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    internal static void Inject(ActivityContext context, IList<MqttUserProperty> carrier)
      => Propagator.Inject(new PropagationContext(context, Baggage.Current), carrier, InjectInternal);

    internal static PropagationContext Extract(IList<MqttUserProperty>? carrier)
    {
        var parentContext = Propagator.Extract(default, carrier, ExtractInternal);
        Baggage.Current = parentContext.Baggage;
        return parentContext;
    }

    private static void InjectInternal(IList<MqttUserProperty> userProperties, string key, string value)
      => userProperties.Add(new MqttUserProperty(key, value));

    private static IEnumerable<string> ExtractInternal(IList<MqttUserProperty>? userProperties, string key)
      => userProperties?
        .Where(property => property.Name.Equals(key, StringComparison.Ordinal))
        .Select(property => property.Value) ?? [];
}
