// <copyright file="ActiveMQMessageContextPropagationTests.cs" company="OpenTelemetry Authors">
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

using System.Linq;
using Apache.NMS;
using Moq;
using Xunit;

namespace OpenTelemetry.Instrumentation.ActiveMQ.Tests;

public class ActiveMQMessageContextPropagationTests
{
    private const string TRACEPARENTKEY = "traceparent";
    private const string TRACEPARENTVALUE = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";

    [Fact]
    public void Inject_WhenNotNull_ShouldSetString()
    {
        var messagePropertiesMock = new Mock<IPrimitiveMap>();

        ActiveMQMessageContextPropagation.InjectTraceContext(messagePropertiesMock.Object, TRACEPARENTKEY, TRACEPARENTVALUE);

        messagePropertiesMock.Verify(mock => mock.SetString(TRACEPARENTKEY, TRACEPARENTVALUE), Times.Once());
        messagePropertiesMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Extract_WhenKeyNotExists_ShouldReturnEmptyEnumerable()
    {
        var messagePropertyMock = new Mock<IPrimitiveMap>();
        messagePropertyMock.Setup(mock => mock.Contains(TRACEPARENTKEY)).Returns(false);

        var result = ActiveMQMessageContextPropagation.ExtractTraceContext(messagePropertyMock.Object, TRACEPARENTKEY);
        Assert.Equal(Enumerable.Empty<string>(), result);
    }

    [Fact]
    public void Extract_WhenKeyExists_ShouldReturnSingleIndexArrayWithKey()
    {
        var messagePropertyMock = new Mock<IPrimitiveMap>();
        messagePropertyMock.Setup(mock => mock.Contains(TRACEPARENTKEY)).Returns(true);
        messagePropertyMock.Setup(mock => mock.GetString(TRACEPARENTKEY)).Returns(TRACEPARENTVALUE);

        var result = ActiveMQMessageContextPropagation.ExtractTraceContext(messagePropertyMock.Object, TRACEPARENTKEY);
        Assert.Equal(TRACEPARENTVALUE, result.First());
    }
}
