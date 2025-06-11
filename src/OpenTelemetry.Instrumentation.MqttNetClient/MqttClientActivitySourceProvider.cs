// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace OpenTelemetry.Instrumentation.MqttNetClient;

internal static class MqttClientActivitySourceProvider
{
    public static readonly string ActivitySourceName = typeof(MqttClientActivitySourceProvider).Assembly.GetName().Name!;
    public static readonly string? ActivitySourceVersion = typeof(MqttClientActivitySourceProvider).Assembly.GetName().Version?.ToString();
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion);
}
