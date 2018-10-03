// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Testing.xunit;

namespace Microsoft.Extensions.Logging.Testing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class RetryTestAttribute : Attribute
    {
        public RetryTestAttribute(int retryCount)
            : this(retryCount, OperatingSystems.Linux | OperatingSystems.MacOSX | OperatingSystems.Windows) { }

        public RetryTestAttribute(int retryCount, OperatingSystems operatingSystems)
            : this(retryCount, operatingSystems, ex => true) { }

        public RetryTestAttribute(int retryCount, Type retryPredicateType)
            : this(retryCount, OperatingSystems.Linux | OperatingSystems.MacOSX | OperatingSystems.Windows, retryPredicateType) { }

        public RetryTestAttribute(int retryCount, OperatingSystems operatingSystems, Type retryPredicateType)
            : this(retryCount, operatingSystems)
        {
            var retryPredicateMethod = retryPredicateType.GetMethod("ShouldRetry",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(Exception) },
                null)
                ?? throw new ArgumentException($"No valid ShouldRetry method was found on {retryPredicateType.Name}.");

            if (retryPredicateMethod.ReturnType != typeof(bool))
            {
                throw new ArgumentException($"ShouldRetry on {retryPredicateType.Name} does not return bool.");
            }

            var parameter = Expression.Parameter(typeof(Exception));
            RetryPredicate = Expression.Lambda<Func<Exception, bool>>(
                Expression.Call(retryPredicateMethod, parameter), parameter).Compile();
        }

        private RetryTestAttribute(int retryCount, OperatingSystems operatingSystems, Func<Exception, bool> retryPredicate)
        {
            if (retryCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count must be positive.");
            }

            RetryCount = retryCount;
            OperatingSystems = operatingSystems;
            RetryPredicate = retryPredicate;
        }

        public Func<Exception, bool> RetryPredicate { get; }

        public int RetryCount { get; }

        public OperatingSystems OperatingSystems { get; }
    }
}
