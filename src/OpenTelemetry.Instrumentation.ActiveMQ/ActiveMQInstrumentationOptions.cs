// <copyright file="ActiveMQInstrumentationOptions.cs" company="OpenTelemetry Authors">
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

namespace OpenTelemetry.Instrumentation.ActiveMQ;

/// <summary>
/// Options for ActiveMQ client instrumentation.
/// </summary>
public class ActiveMQInstrumentationOptions
{
    /// <summary>
    /// Gets or sets an action to enrich an Activity.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Activity"/>: the activity being enriched.</para>
    /// <para>string: the name of the event.</para>
    /// <para>object: the raw <c>SqlCommand</c> object from which additional information can be extracted to enrich the activity.</para>
    /// <para>See also: <a href="https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Instrumentation.SqlClient#Enrich">example</a>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    ///     .AddSqlClientInstrumentation(opt => opt.Enrich
    ///         = (activity, eventName, rawObject) =>
    ///      {
    ///         if (eventName.Equals("OnCustom"))
    ///         {
    ///             if (rawObject is SqlCommand cmd)
    ///             {
    ///                 activity.SetTag("db.commandTimeout", cmd.CommandTimeout);
    ///             }
    ///         }
    ///      })
    ///     .Build();
    /// </code>
    /// </example>
    public Action<Activity, string, object> Enrich { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception will be recorded as ActivityEvent or not. Default value: False.
    /// </summary>
    /// <remarks>
    /// https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/exceptions.md.
    /// </remarks>
    public bool RecordException { get; set; }
}
