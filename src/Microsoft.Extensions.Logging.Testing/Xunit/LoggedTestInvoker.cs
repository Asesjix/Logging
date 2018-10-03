// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Extensions.Logging.Testing
{
    public class LoggedTestInvoker : XunitTestInvoker
    {
        private readonly ITestOutputHelper _output;

        public LoggedTestInvoker(
            ITest test,
            IMessageBus messageBus,
            Type testClass,
            object[] constructorArguments,
            MethodInfo testMethod,
            object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            ITestOutputHelper output)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _output = output;
        }

        protected override object CreateTestClass()
        {
            var testClass = base.CreateTestClass();

            (testClass as ILoggedTest).Initialize(
                TestMethod,
                TestMethodArguments,
                _output ?? ConstructorArguments.SingleOrDefault(a => typeof(ITestOutputHelper).IsAssignableFrom(a.GetType())) as ITestOutputHelper);

            return testClass;
        }

        protected override async Task<decimal> InvokeTestMethodAsync(object testClassInstance)
        {
            var loggedTestBase = testClassInstance as LoggedTestBase;
            for (int i = 0; i < (loggedTestBase?.TestRetries ?? 1); i++)
            {
                if (i > 0)
                {
                    if (!loggedTestBase.RetryPredicate(Aggregator.ToException()))
                    {
                        return Timer.Total;
                    }

                    // This can only occur if the retry count has been overridden on the LoggedTestBase class
                    loggedTestBase.Logger.LogWarning($"{TestMethod.Name} failed and retries are enabled, re-executing.");
                }

                Aggregator.Clear();
                await base.InvokeTestMethodAsync(testClassInstance);

                if (!Aggregator.HasExceptions)
                {
                    break;
                }
            }

            return Timer.Total;
        }
    }
}
