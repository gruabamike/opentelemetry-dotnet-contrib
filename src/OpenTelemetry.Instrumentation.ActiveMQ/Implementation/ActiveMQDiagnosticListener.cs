// <copyright file="ActiveMQDiagnosticListener.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Apache.NMS;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.ActiveMQ.Implementation;

internal sealed class ActiveMQDiagnosticListener : ListenerHandler
{
    public const string ActiveMQEventNamePrefix = "MessageBroker.ActiveMQ.AutoInstrumentation.";
    public const string BeforeMessageSend = ActiveMQEventNamePrefix + "WriteMessageSendBefore";
    public const string AfterMessageSend = ActiveMQEventNamePrefix + "WriteMessageSendAfter";
    public const string BeforeMessageReceive = ActiveMQEventNamePrefix + "WriteMessageReceiveBefore";
    public const string AfterMessageReceive = ActiveMQEventNamePrefix + "WriteMessageReceiveAfter";

    public const string ErrorMessageSend = ActiveMQEventNamePrefix + "MessageSendError";
    public const string ErrorMessageReceive = ActiveMQEventNamePrefix + "MessageReceiveError";

    private readonly PropertyFetcher<IConnectionFactory?> connectionFactoryFetcher = new("ConnectionFactory");
    private readonly PropertyFetcher<IConnection?> connectionFetcher = new("Connection");
    private readonly PropertyFetcher<IMessage?> messageFetcher = new("Message");
    private readonly PropertyFetcher<Exception?> exceptionFetcher = new("Exception");

    private readonly ActiveMQInstrumentationOptions options;

    public ActiveMQDiagnosticListener(
        string sourceName,
        ActiveMQInstrumentationOptions options)
        : base(sourceName)
    {
        this.options = options ?? new ActiveMQInstrumentationOptions();
    }

    public override bool SupportsNullActivity => true;

    public override void OnCustom(string name, Activity activity, object payload)
    {
        switch (name)
        {
            case BeforeMessageSend:
            {
                activity = ActiveMQSourceInfoProvider.ActivitySource.StartActivity(
                    ActiveMQSourceInfoProvider.ActivityName,
                    ActivityKind.Producer,
                    default(ActivityContext),
                    ActiveMQSourceInfoProvider.CreationTags);

                if (activity is null)
                {
                    // There is no listener or the sampler decided not to sample the current request
                    return;
                }

                _ = this.messageFetcher.TryFetch(payload, out var message);
                if (message is null)
                {
                    ActiveMQInstrumentationEventSource.Log.NullPayload(nameof(ActiveMQDiagnosticListener), name);
                    activity.Stop();
                    return;
                }

                ActivityContext contextToInject = default;
                if (activity is not null)
                {
                    contextToInject = activity.Context;
                }
                else if (Activity.Current is not null)
                {
                    contextToInject = Activity.Current.Context;
                }

                // Propagate context irrespective of sampling decision
                var textMapPropagator = Propagators.DefaultTextMapPropagator;
                textMapPropagator.Inject(
                    new PropagationContext(contextToInject, Baggage.Current),
                    message.Properties,
                    ActiveMQMessageContextPropagation.InjectTraceContext);

                // Enrich activity from payload only if sampling decision is favourable
                if (activity.IsAllDataRequested)
                {
                    _ = this.connectionFactoryFetcher.TryFetch(payload, out var connectionFactory);
                    this.SetMessagingActivityDetails(activity, connectionFactory, message);

                    try
                    {
                        this.options.Enrich?.Invoke(activity, nameof(this.OnCustom), message);
                    }
                    catch (Exception e)
                    {
                        ActiveMQInstrumentationEventSource.Log.EnrichmentException(e);
                    }
                }

                break;
            }

            case BeforeMessageReceive:
            {
                _ = this.messageFetcher.TryFetch(payload, out var message);
                if (message is null)
                {
                    ActiveMQInstrumentationEventSource.Log.NullPayload(nameof(ActiveMQDiagnosticListener), name);
                    return;
                }

                // Propagate context irrespective of sampling decision
                var textMapPropagator = Propagators.DefaultTextMapPropagator;
                var parentContext = textMapPropagator.Extract(
                    default,
                    message.Properties,
                    ActiveMQMessageContextPropagation.ExtractTraceContext);
                Baggage.Current = parentContext.Baggage;

                activity = ActiveMQSourceInfoProvider.ActivitySource.StartActivity(
                    ActiveMQSourceInfoProvider.ActivityName,
                    ActivityKind.Consumer,
                    parentContext.ActivityContext,
                    ActiveMQSourceInfoProvider.CreationTags);

                if (activity is null)
                {
                    // There is no listener or the sampler decided not to sample the current request
                    return;
                }

                // Enrich activity from payload only if sampling decision is favourable
                if (activity.IsAllDataRequested)
                {
                    _ = this.connectionFactoryFetcher.TryFetch(payload, out var connectionFactory);
                    this.SetMessagingActivityDetails(activity, connectionFactory, message);

                    _ = this.connectionFetcher.TryFetch(payload, out var connection);
                    activity.SetTag(TraceSemanticConventions.AttributeMessagingOperation, TraceSemanticConventions.MessagingOperationValues.Receive);
                    activity.SetTag(TraceSemanticConventions.AttributeMessagingConsumerId, connection?.ClientId ?? "unknown");

                    try
                    {
                        this.options.Enrich?.Invoke(activity, nameof(this.OnCustom), message);
                    }
                    catch (Exception e)
                    {
                        ActiveMQInstrumentationEventSource.Log.EnrichmentException(e);
                    }
                }

                break;
            }

            case AfterMessageSend:
            case AfterMessageReceive:
            {
                if (activity is null)
                {
                    ActiveMQInstrumentationEventSource.Log.NullActivity(name);
                    return;
                }

                if (activity.Source != ActiveMQSourceInfoProvider.ActivitySource)
                {
                    return;
                }

                try
                {
                    // Enrich activity from payload only if sampling decision is favourable
                    if (activity.IsAllDataRequested)
                    {
                        activity.SetStatus(Status.Unset);
                    }
                }
                finally
                {
                    activity.Stop();
                }

                break;
            }

            case ErrorMessageSend:
            case ErrorMessageReceive:
            {
                if (activity is null)
                {
                    ActiveMQInstrumentationEventSource.Log.NullActivity(name);
                    return;
                }

                if (activity.Source != ActiveMQSourceInfoProvider.ActivitySource)
                {
                    return;
                }

                try
                {
                    // Enrich activity from payload only if sampling decision is favourable
                    if (activity.IsAllDataRequested)
                    {
                        if (this.exceptionFetcher.TryFetch(payload, out Exception exception) && exception is not null)
                        {
                            activity.SetStatus(Status.Error.WithDescription(exception.Message));

                            if (this.options.RecordException)
                            {
                                activity.RecordException(exception);
                            }
                        }
                        else
                        {
                            ActiveMQInstrumentationEventSource.Log.NullPayload(nameof(ActiveMQDiagnosticListener), name);
                        }
                    }
                }
                finally
                {
                    activity.Stop();
                }

                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetMessagingActivityDetails(
        Activity activity,
        IConnectionFactory? connectionFactory,
        IMessage message)
    {
        var (destinationName, destinationKind) = this.GetMessageDestinationValues(message.NMSDestination);
        activity.DisplayName = destinationName;

        activity.SetTag(TraceSemanticConventions.AttributeMessagingDestination, destinationName);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingDestinationKind, destinationKind);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingTempDestination, message.NMSDestination.IsTemporary);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingUrl, connectionFactory?.BrokerUri?.ToString() ?? "unkown");
        activity.SetTag(TraceSemanticConventions.AttributeMessagingMessageId, message.NMSMessageId);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingConversationId, message.NMSCorrelationID);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (string DestinationName, string DestinationKindValue) GetMessageDestinationValues(IDestination destination)
    {
        switch (destination.DestinationType)
        {
            case DestinationType.Topic:
            case DestinationType.TemporaryTopic:
                return (((ITopic)destination).TopicName, TraceSemanticConventions.MessagingDestinationKindValues.Topic);
            case DestinationType.Queue:
            case DestinationType.TemporaryQueue:
            default:
                return (((IQueue)destination).QueueName, TraceSemanticConventions.MessagingDestinationKindValues.Queue);
        }
    }
}
