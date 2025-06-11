// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using MQTTnet;
using MQTTnet.Packets;

namespace OpenTelemetry.Instrumentation.MqttNetClient.Decorators;

internal sealed class MqttClientDecoratorTracing : MqttClientDecoratorBase
{
    public MqttClientDecoratorTracing(IMqttClient mqttClient)
        : base(mqttClient)
    {
    }

    public override Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
    {
        using var activity = MqttClientActivitySourceProvider.ActivitySource.StartActivity(
          MqttClientActivityHelper.GetActivityNamePublish(applicationMessage.Topic),
          ActivityKind.Producer,
          default(ActivityContext),
          MqttClientActivityHelper.PublishTags(applicationMessage, this.Options));

        if (activity != null)
        {
            applicationMessage.UserProperties ??= new List<MqttUserProperty>();
            MqttClientContextPropagationHandler.Inject(activity.Context, applicationMessage.UserProperties);

            if (activity.IsAllDataRequested)
            {
                MqttClientActivityHelper.AddAdditionalTags(activity, applicationMessage, this.Options);
            }
        }

        return base.PublishAsync(applicationMessage, cancellationToken);
    }

    protected override Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var parentContext = MqttClientContextPropagationHandler.Extract(e.ApplicationMessage.UserProperties);
        using var activity = MqttClientActivitySourceProvider.ActivitySource.StartActivity(
          MqttClientActivityHelper.GetActivityNameConsume(e.ApplicationMessage.Topic),
          ActivityKind.Consumer,
          parentContext.ActivityContext,
          MqttClientActivityHelper.ConsumeTags(e.ApplicationMessage, this.Options));

        if (activity != null && activity.IsAllDataRequested)
        {
            MqttClientActivityHelper.AddAdditionalTags(activity, e.ApplicationMessage, this.Options);
        }

        return base.OnApplicationMessageReceivedAsync(e);
    }
}
