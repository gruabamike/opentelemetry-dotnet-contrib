// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using MQTTnet;
using MQTTnet.Diagnostics.PacketInspection;

namespace OpenTelemetry.Instrumentation.MqttNetClient.Decorators;

internal abstract class MqttClientDecoratorBase : IMqttClient
{
    protected readonly IMqttClient mqttClient;
    private bool disposed;

    protected MqttClientDecoratorBase(IMqttClient mqttClient)
    {
        this.mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));

        // delegate events from the inner mqttClient to the methods that can be extended
        this.mqttClient.ApplicationMessageReceivedAsync += this.OnApplicationMessageReceivedAsync;
        this.mqttClient.ConnectedAsync += this.OnConnectedAsync;
        this.mqttClient.ConnectingAsync += this.OnConnectingAsync;
        this.mqttClient.DisconnectedAsync += this.OnDisconnectedAsync;
        this.mqttClient.InspectPacketAsync += this.OnInspectPacketAsync;
    }

    public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync;

    public event Func<MqttClientConnectedEventArgs, Task>? ConnectedAsync;

    public event Func<MqttClientConnectingEventArgs, Task>? ConnectingAsync;

    public event Func<MqttClientDisconnectedEventArgs, Task>? DisconnectedAsync;

    public event Func<InspectMqttPacketEventArgs, Task>? InspectPacketAsync;

    public bool IsConnected => this.mqttClient.IsConnected;

    public MqttClientOptions Options => this.mqttClient.Options;

    public virtual Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
        => this.mqttClient.ConnectAsync(options, cancellationToken);

    public virtual Task DisconnectAsync(MqttClientDisconnectOptions options, CancellationToken cancellationToken = default)
        => this.mqttClient.DisconnectAsync(options, cancellationToken);

    public virtual Task PingAsync(CancellationToken cancellationToken = default)
        => this.mqttClient.PingAsync(cancellationToken);

    public virtual Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
        => this.mqttClient.PublishAsync(applicationMessage, cancellationToken);

    public virtual Task SendEnhancedAuthenticationExchangeDataAsync(MqttEnhancedAuthenticationExchangeData data, CancellationToken cancellationToken = default)
        => this.mqttClient.SendEnhancedAuthenticationExchangeDataAsync(data, cancellationToken);

    public virtual Task<MqttClientSubscribeResult> SubscribeAsync(MqttClientSubscribeOptions options, CancellationToken cancellationToken = default)
        => this.mqttClient.SubscribeAsync(options, cancellationToken);

    public virtual Task<MqttClientUnsubscribeResult> UnsubscribeAsync(MqttClientUnsubscribeOptions options, CancellationToken cancellationToken = default)
        => this.mqttClient.UnsubscribeAsync(options, cancellationToken);

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        if (this.ApplicationMessageReceivedAsync != null)
        {
            await this.ApplicationMessageReceivedAsync.Invoke(e).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
    {
        if (this.ConnectedAsync != null)
        {
            await this.ConnectedAsync.Invoke(e).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnConnectingAsync(MqttClientConnectingEventArgs e)
    {
        if (this.ConnectingAsync != null)
        {
            await this.ConnectingAsync.Invoke(e).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
    {
        if (this.DisconnectedAsync != null)
        {
            await this.DisconnectedAsync.Invoke(e).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnInspectPacketAsync(InspectMqttPacketEventArgs e)
    {
        if (this.InspectPacketAsync != null)
        {
            await this.InspectPacketAsync.Invoke(e).ConfigureAwait(false);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                // dispose managed objects here
                this.mqttClient?.Dispose();
            }

            // dispose unmanaged objects here
            this.disposed = true;
        }
    }
}
