# MQTTnet Client Instrumentation for OpenTelemetry

| Status        |           |
| ------------- |-----------|
| Stability     |  [Beta](../../README.md#beta)|
| Code Owners   |  [@gruabamike](https://github.com/gruabamike)|

[![NuGet version badge](https://img.shields.io/nuget/v/OpenTelemetry.Instrumentation.MqttNetClient)](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.MqttNetClient)
[![NuGet download count badge](https://img.shields.io/nuget/dt/OpenTelemetry.Instrumentation.MqttNetClient)](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.MqttNetClient)

This is an
[Instrumentation Library](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/glossary.md#instrumentation-library),
which instruments
[MQTTnet client](https://www.nuget.org/packages/MQTTnet/)
and produces and collects spans about message exchange using `IMqttClient`.

> [!NOTE]
> This component is based on the OpenTelemetry semantic conventions for
[traces](https://github.com/open-telemetry/semantic-conventions/blob/main/docs/database/redis.md).
These conventions are
[Experimental](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/document-status.md),
and hence, this package is a [pre-release](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/VERSIONING.md#pre-releases).
Until a [stable
version](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/telemetry-stability.md)
is released, there can be breaking changes.

## Steps to enable OpenTelemetry.Instrumentation.MqttNetClient

## Step 1: Install Package

Add a reference to the
[`OpenTelemetry.Instrumentation.MqttNetClient`](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.MqttNetClient)
package. Also, add any other instrumentations & exporters you will need.

```shell
dotnet add package OpenTelemetry.Instrumentation.MqttNetClient
```

## Step 2: Enable MQTTnet client Instrumentation at application startup

## References

* [OpenTelemetry Project](https://opentelemetry.io/)
* [MQTTnet](https://github.com/dotnet/MQTTnet)
