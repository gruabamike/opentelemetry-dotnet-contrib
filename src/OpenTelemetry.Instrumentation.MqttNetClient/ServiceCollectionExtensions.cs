// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MQTTnet;
using OpenTelemetry.Instrumentation.MqttNetClient.Decorators;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Instrumentation.MqttNetClient;

/// <summary>
/// Extension methods to simplify registering of the decorated mqtt client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces the existing <see cref="IMqttClient"/> registration with a singleton
    /// instance of <see cref="MqttClientDecoratorTracing"/> using the default MQTT client factory.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMqttNetClientDecoratorTracing(this IServiceCollection services)
      => services.AddMqttNetClientDecoratorTracing(() => new MqttClientFactory().CreateMqttClient());

    /// <summary>
    /// Replaces the existing <see cref="IMqttClient"/> registration with a singleton
    /// instance of <see cref="MqttClientDecoratorTracing"/>, using a custom MQTT client provider.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="mqttClientProvider">A factory method to create the underlying <see cref="IMqttClient"/>.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMqttNetClientDecoratorTracing(this IServiceCollection services, Func<IMqttClient> mqttClientProvider)
    {
        Guard.ThrowIfNull(services);
        Guard.ThrowIfNull(mqttClientProvider);

        return services.Replace(ServiceDescriptor.Singleton<IMqttClient>(sp =>
          new MqttClientDecoratorTracing(mqttClientProvider())));
    }
}
