// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Logging.Testing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class RetryTestAttribute : Attribute
    {
        public RetryTestAttribute(int retryCount)
        {
            if (retryCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count must be positive.");
            }

            RetryCount = retryCount;
        }

        public int RetryCount { get; }
    }
}
