// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using MQTTnet;

namespace OpenTelemetry.Instrumentation.MqttNetClient;

internal static class MqttClientActivityHelper
{
    public static IEnumerable<KeyValuePair<string, object?>> PublishTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
        .. CommonTags(msg, opt),
        new(MqttMessagingSpanSemanticConventions.AttributeMessagingOperationName, MqttMessagingSpanSemanticConventions.MessagingOperationNamePublish),
        new(MqttMessagingSpanSemanticConventions.AttributeMessagingOperationType, MqttMessagingSpanSemanticConventions.MessagingOperationTypeSend),
    ];

    public static IEnumerable<KeyValuePair<string, object?>> ConsumeTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
        .. CommonTags(msg, opt),
        new(MqttMessagingSpanSemanticConventions.AttributeMessagingOperationName, MqttMessagingSpanSemanticConventions.MessagingOperationNameConsume),
        new(MqttMessagingSpanSemanticConventions.AttributeMessagingOperationType, MqttMessagingSpanSemanticConventions.MessagingOperationTypeReceive),
    ];

    public static string GetActivityNamePublish(string topic) => $"{MqttMessagingSpanSemanticConventions.MessagingOperationNamePublish} {topic}";

    public static string GetActivityNameConsume(string topic) => $"{MqttMessagingSpanSemanticConventions.MessagingOperationNameConsume} {topic}";

    public static void AddAdditionalTags(
        Activity activity,
        MqttApplicationMessage message,
        MqttClientOptions options)
    {
        activity?.SetTag(MqttMessagingSpanSemanticConventions.AttributeMessagingMessageBodySize, message.Payload.Length);
        activity?.SetTag(MqttMessagingSpanSemanticConventions.AttributeMessagingClientId, options.ClientId);
        activity?.SetTag(MqttMessagingSpanSemanticConventions.AttributeMessagingMqttRetain, message.Retain);
        activity?.SetTag(MqttMessagingSpanSemanticConventions.AttributeMessagingMqttQoS, (int)message.QualityOfServiceLevel);
        activity?.SetTag(MqttMessagingSpanSemanticConventions.AttributeMessagingMqttTopicAlias, message.TopicAlias);
        activity?.SetTag(MqttMessagingSpanSemanticConventions.AttributeMessagingMqttProtocolVersion, (int)options.ProtocolVersion);
    }

    private static IEnumerable<KeyValuePair<string, object?>> CommonTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
        new(MqttMessagingSpanSemanticConventions.AttributeMessagingSystem, MqttMessagingSpanSemanticConventions.MessagingSystemMqtt),
        new(MqttMessagingSpanSemanticConventions.AttributeMessagingDestinationName, msg.Topic),
        new(MqttMessagingSpanSemanticConventions.AttributeServerAddress, opt.ChannelOptions.GetHost()),
        new(MqttMessagingSpanSemanticConventions.AttributeServerPort, opt.ChannelOptions.GetPort()),
    ];
}
