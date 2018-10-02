// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace Microsoft.Extensions.Logging.Testing.Tests
{
    [RetryTest(2)]
    public class LoggedTestXunitRetryTests : LoggedTest
    {
        public LoggedTestXunitRetryTests()
        {
            RetryCounter.Reset();
        }

        [Fact]
        public void CompletesWithoutRetryOnSuccess()
        {
            Assert.Equal(2, TestRetries);

            // This assert would fail on the second run
            Assert.Equal(0, TestSink.Writes.Count);
        }

        [Fact]
        public void RetriesUntilSuccess()
        {
            // This assert will fail the first time but pass on the second
            Assert.Equal(1, RetryCounter.RetryCount);

            // This assert will ensure the test ran twice.
            Assert.Equal(1, TestSink.Writes.Count);
            var loggedMessage = TestSink.Writes.ToArray()[0];
            Assert.Equal(LogLevel.Warning, loggedMessage.LogLevel);
            Assert.Equal($"{nameof(RetriesUntilSuccess)} failed and retries are enabled, re-executing.", loggedMessage.Message);
        }

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Windows)]
        [RetryTest(3, OperatingSystems.Windows)]
        public void RetryCountNotOverridenWhenOSDoesNotMatch()
        {
            Assert.Equal(2, TestRetries);
        }

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
        [RetryTest(3, OperatingSystems.Windows)]
        public void RetryCountOverridenWhenOSMatches()
        {
            Assert.Equal(3, TestRetries);
        }
    }

    public static class RetryCounter
    {
        private static int _retryCount;

        public static int RetryCount
        {
            get
            {
                return _retryCount++;
            }
        }

        public static void Reset()
        {
            _retryCount = 0;
        }
    }
}
