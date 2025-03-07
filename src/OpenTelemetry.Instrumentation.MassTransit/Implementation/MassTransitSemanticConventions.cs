// <copyright file="MassTransitSemanticConventions.cs" company="OpenTelemetry Authors">
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

namespace OpenTelemetry.Instrumentation.MassTransit.Implementation;

internal class MassTransitSemanticConventions
{
    public const string AttributeMessagingMassTransitInitiatorId = "messaging.masstransit.initiator_id";
    public const string AttributeMessagingMassTransitCorrelationId = "messaging.masstransit.correlation_id";
    public const string AttributeMessagingMassTransitConsumerType = "messaging.masstransit.consumer_type";
}
