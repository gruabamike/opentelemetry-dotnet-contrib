// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using MQTTnet;

namespace OpenTelemetry.Instrumentation.MqttNetClient;

/// <summary>
/// Options for MqttNetClient instrumentation.
/// </summary>
public class MqttClientInstrumentationOptions
{
    /// <summary>
    /// Gets or sets an action to enrich an Activity.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Activity"/>: the activity being enriched.</para>
    /// <para><see cref="MqttApplicationMessage"/>: the mqtt application message from which additional information can be extracted to enrich the activity.</para>
    /// </remarks>
    public Action<Activity, MqttApplicationMessage>? Enrich { get; set; }
}
