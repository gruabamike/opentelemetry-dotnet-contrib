// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using OpenTelemetry.Instrumentation.MqttNetClient;
using OpenTelemetry.Instrumentation.MqttNetClient.Decorators;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Trace;

/// <summary>
/// Extension methods to simplify registering of dependency instrumentation.
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Adds OpenTelemetry instrumentation support for MQTTnet client operations.
    /// Requires that the provided or registered <see cref="IMqttClient"/> is a <see cref="MqttClientDecoratorTracing"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <param name="mqttClient">An optional MQTT client instance to validate. If null, the instance is resolved from the service provider.</param>
    /// <returns>The updated <see cref="TracerProviderBuilder"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resolved MQTT client is not decorated for tracing.</exception>
    public static TracerProviderBuilder AddMqttNetClientInstrumentation(
        this TracerProviderBuilder builder,
        IMqttClient? mqttClient = null)
    {
        Guard.ThrowIfNull(builder);

        if (builder is not IDeferredTracerProviderBuilder deferredTracerProviderBuilder)
        {
            if (mqttClient is null)
            {
                throw new InvalidOperationException("IMqttClient is not provided and cannot be resolved at this stage.");
            }

            if (mqttClient is not MqttClientDecoratorTracing)
            {
                throw new InvalidOperationException(
                    $"The provided IMqttClient must be of type {nameof(MqttClientDecoratorTracing)} to enable tracing.");
            }

            return AddMqttNetClientInstrumentationInternal(builder);
        }

        return deferredTracerProviderBuilder.Configure((serviceProvider, configuredBuilder) =>
        {
            var resolvedClient = mqttClient ?? serviceProvider.GetService<IMqttClient>();

            if (resolvedClient is not MqttClientDecoratorTracing)
            {
                throw new InvalidOperationException(
                    $"The registered IMqttClient must be of type {nameof(MqttClientDecoratorTracing)}. " +
                    "Ensure you have called AddMqttNetClientDecoratorTracing() in your service registration.");
            }

            AddMqttNetClientInstrumentationInternal(configuredBuilder);
        });
    }

    private static TracerProviderBuilder AddMqttNetClientInstrumentationInternal(TracerProviderBuilder builder)
    {
        return builder.AddSource(MqttClientActivitySourceProvider.ActivitySourceName);
    }
}
